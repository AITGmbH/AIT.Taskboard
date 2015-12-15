using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Xml;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.Reporting;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.ObjectModel;
using QueryDefinition = Microsoft.TeamFoundation.WorkItemTracking.Client.QueryDefinition;
using System.Windows;
using System.Windows.Threading;

namespace AIT.Taskboard.Model
{
    [Export(typeof(ITfsInformationProvider))]
    [Export(typeof (ITaskboardService))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TaskboardService : ITaskboardService, ITfsInformationProvider
    {
        #region Fields and Constants

        private ObservableCollection<IBacklogChildren> _backlogChildren;
        
        #endregion

        #region Public Properties

        public WorkItemCollection WorkItems { get; private set; }
        public WorkItemLinkInfo[] WorkItemLinkInfos { get; private set; }
        public List<WorkItem> BackLogItems { get; private set; }
        public List<WorkItem> AllChildren { get; private set; }
        public ILoginData LoginData { get; set; }
        public string ReportFolder { get; private set; }
        public string ReportServerUrl { get; private set; }
        public bool IsConnected
        {
            get { return (TeamProjectCollection != null) &&(TeamProject != null); }
        }
        public Project TeamProject { get; set; }
        public TfsTeamProjectCollection TeamProjectCollection { get; set; }
        public ObservableCollection<IBacklogChildren> BacklogChildren
        {
            get { return _backlogChildren ?? (_backlogChildren = new ObservableCollection<IBacklogChildren>()); }
        }
        #endregion

        #region Private Properties

        private Dictionary<string, List<Transition>> WorkItemTransitions { get; set; }
        [Import]
        private Lazy<ISyncService> SyncService {get;set;}
        [Import]
        public IConfigurationService ConfigurationService { get; set; }
        [Import]
        private ILogger Logger { get; set; }
        [Import]
        public IStatusService StatusService { get; set; }
        
        #endregion

        #region Public functions

        public void Connect()
        {
            IStatusItem statusItem = null;
            try
            {
                var args = new LoginDataEventArgs { LoginData = LoginData };
                OnRequestLoginData(args);
                // If the request was canceled we will also cancel are connect attempt
                if (!args.Cancel)
                {
                    statusItem = StatusService.EnqueueStatusItem("AutoConnectMessage");
                    statusItem.IsProgressing = true;
                    statusItem.IsProgressIndeterminate = true;
                    statusItem.Message = string.Format("Connecting to project {0} on server {1}.", LoginData.TeamProjectName,
                                               LoginData.TeamProjectCollectionUri);

                    TeamProjectCollection = TfsTeamProjectCollectionFactory.GetTeamProjectCollection(LoginData.TeamProjectCollectionUri, new UICredentialsProvider());
                    TeamProjectCollection.EnsureAuthenticated();

                    var workItemStore = TeamProjectCollection.GetService<WorkItemStore>();
                    TeamProject = workItemStore.Projects[LoginData.TeamProjectName];

                    InitReportingServiceProxy();

                    OnConnected();
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                OnConnectionFailed(ex.Message);
            }
            finally
            {
                if(statusItem != null)
                    StatusService.DequeueStatusItem(statusItem);
                FlushWindowsMessageQueue(false);
            }
        }
        public void ApplyConfiguration(IConfiguration configuration)
        {
            if (TeamProjectCollection == null) return;
            if (configuration == null) return;
            if (configuration.QueryId == Guid.Empty)
            {
                var errorMessage = StatusService.EnqueueStatusItem("InvalidQuery");
                errorMessage.Message = "There is no query defined in the configuration. Please choose a query by clicking \"Edit\"";
                return;
            }

            // Before refreshing the data we should verify whether there are unsaved changes that need to be saved
            if (!VerifySaveRequest()) return;

            var statusItem = StatusService.EnqueueStatusItem("ApplyConfiguration");
            statusItem.Message = string.Format("Applying configuration for team project {0}", TeamProject.Name);
            statusItem.IsProgressing = true;
            statusItem.IsProgressIndeterminate = true;
            // Run the query in order to provide the result data
            var store = TeamProjectCollection.GetService<WorkItemStore>();
            // Get the query based on the Id we keep in the configuration
            QueryDefinition queryDefinition = SaveGetQueryDefinition(store, configuration.QueryId);
            Query query = GetQuery(queryDefinition);
            // Only if the query is a link query we perform the fullblown request for data
            if ((query != null)&&(query.IsLinkQuery))
            {
                // First get the linking information
                WorkItemLinkInfos = query.RunLinkQuery();
                // Get the configured link type in order to respect it when retrieving the work items
                var linkType = (!string.IsNullOrEmpty(configuration.LinkType)) ? store.WorkItemLinkTypes.FirstOrDefault(type => type.ReferenceName == configuration.LinkType) : null;
                // Now put all TargetIds into a BatchReadParameterCollection in order to get the work items for the links
                // Attention: Consider handling the link types correctly
                var batchReadParams = new BatchReadParameterCollection();
                foreach (var linkInfo in WorkItemLinkInfos)
                {
                    if (linkType == null)
                    {
                        // If there is no link type there is nothing we can check explicitly and therefor we respect any child
                        if (!batchReadParams.Contains(linkInfo.TargetId))
                            batchReadParams.Add(new BatchReadParameter(linkInfo.TargetId));
                    }
                    else
                    {
                        // Debug.WriteLink("Link: {0} Source: {1} Target: {2} RefName: {3} Forward: {4} Reverse: {5}", linkInfo.LinkTypeId, linkInfo.SourceId, linkInfo.TargetId, linkType.ReferenceName, linkType.ForwardEnd.Id, linkType.ReverseEnd.Id);
                        // We need to respect the link type.
                        if (IsValidLinkInfo(linkInfo, linkType))
                        {
                            // When the link info is valid according to the current configuration then we consider the work item in our query.
                            if (!batchReadParams.Contains(linkInfo.TargetId))
                                batchReadParams.Add(new BatchReadParameter(linkInfo.TargetId));
                        }
                        // When the link type is not valid we also do not consider the item.
                    }
                }
                // Now construct the query to get the work items
                string batchQuery = "SELECT {0} FROM WorkItems";
                List<string> displayFields = (from FieldDefinition fieldDefinition in query.DisplayFieldList select fieldDefinition.ReferenceName).ToList();
                string displayFieldPart = string.Join(",", displayFields.ToArray());
                if (batchReadParams.Count != 0)
                {
                    batchQuery = string.Format(batchQuery, displayFieldPart);
                    // Run the query and remember the results
                    WorkItems = store.Query(batchReadParams, batchQuery);
                    BuildBacklogItemsList(configuration);
                    BuildChildItemsList(configuration);
                }
                else
                    ApplyChildrenChangesOnUIThread(new List<BacklogChildren>());
            }
            else
            {
                WorkItemLinkInfos = null;
                WorkItems = query != null ? query.RunQuery() : null;
                if (AllChildren != null) AllChildren.Clear();
                if (BacklogChildren.Count > 0) BacklogChildren.Clear();
                if (BackLogItems != null) BackLogItems.Clear();
            }
            PrepareTransitionsMap(configuration);
            OnConfigurationApplied();
        }
        public WorkItemTypeCollection GetWorkItemTypes()
        {
            if (TeamProject != null)
            {
                return TeamProject.WorkItemTypes;
            }
            return null;
        }
        public QueryHierarchy GetWorkItemQueries(bool refreshQueryHierarchy)
        {
            if (TeamProject != null)
            {
                try
                {
                    if (refreshQueryHierarchy)
                        TeamProject.QueryHierarchy.Refresh();
                    return TeamProject.QueryHierarchy;
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    HandleConnectionException(ex);
                    throw;
                }
            }
            return null;
        }

        public ICollection<Node> GetIterationPaths()
        {
            if (TeamProject != null)
            {
                List<Node> result = new List<Node>();
                if (TeamProject.IterationRootNodes.Count > 0)
                {
                    result.Add(TeamProject.IterationRootNodes[0].ParentNode);
                }
                return result;
            }
            return null;
        }

        public ICollection<Node> GetAreaPaths()
        {
            if (TeamProject != null)
            {
                List<Node> result = new List<Node>();
                if (TeamProject.AreaRootNodes.Count > 0)
                {
                    result.Add(TeamProject.AreaRootNodes[0].ParentNode);
                }
                return result;
            }
            return null;
        }

        public List<FieldDefinition> GetPossibleSummaryFields(QueryItem queryItem)
        {
            // For a given query item: Get its definition and extract all numeric fields returned by the query
            Query query = GetQuery(queryItem);
            return query != null
                       ? query.DisplayFieldList.Cast<FieldDefinition>().Where(
                           field => (field.FieldType == FieldType.Integer) || (field.FieldType == FieldType.Double)).
                             ToList()
                       : null;
        }
        public List<FieldDefinition> GetPossibleSortFields(QueryItem queryItem)
        {
            // For a given query item: Get its definition and extract all fields returned by the query
            Query query = GetQuery(queryItem);
            return query != null ? query.DisplayFieldList.Cast<FieldDefinition>().ToList() : null;
        }
        public WorkItemLinkTypeCollection GetLinkTypes()
        {
            try
            {
                if (TeamProjectCollection == null)
                    return null;
                var store = TeamProjectCollection.GetService<WorkItemStore>();
                return store != null ? store.WorkItemLinkTypes : null;
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                HandleConnectionException(ex);
                throw;
            }

        }
        public string GetTargetState(WorkItem workItem, ICustomState targetState)
        {
            if (WorkItemTransitions == null) return null;
            if (!WorkItemTransitions.ContainsKey(workItem.Type.Name))
            {
                var witName = workItem.Type.Name;
                List<Transition> witTransitions = GetTransitions(witName);
                WorkItemTransitions.Add(witName, witTransitions);
            }
            var transitions = WorkItemTransitions[workItem.Type.Name];
            if (transitions == null) return null;

            var stateField = workItem.Fields[CoreField.State];
            // If the target state is the same state as the original state value of the work item it means we move a changed work item back to its original state
            if (targetState.WorkItemStates.Contains(stateField.OriginalValue))
                return (string)stateField.OriginalValue;

            var possibleStates = from Transition transition in transitions
                                 where transition.FromState == (string) stateField.OriginalValue && targetState.WorkItemStates.Contains(transition.ToState)
                                 select transition.ToState;
            return possibleStates.Count() > 0 ? possibleStates.ElementAt(0) : null;
        }
        public QueryItem GetQueryItem(Guid queryId)
        {
            if (TeamProject != null)
            {
                QueryItem item = TeamProject.QueryHierarchy.Find(queryId);
                if (item != null)
                    return item;
                throw new ArgumentException("Invalid queryId", "queryId");
            }
            return null;
        }
        public List<IHierarchicalItem> GetReports()
        {
            var proxy = InitReportingServiceProxy();

            if (proxy == null) return null;

            var items = new List<IHierarchicalItem>();
            
            if (string.IsNullOrEmpty(ReportServerUrl)) return items;
            if (string.IsNullOrEmpty(ReportFolder)) return items;
            
            GetReportsFromServer(items, proxy);
            return items;
        }

        private ReportingServiceProxy InitReportingServiceProxy()
        {
            if (TeamProjectCollection == null) return null;

            var regService = TeamProjectCollection.GetService<IRegistration>();
            var proxy = new ReportingServiceProxy(TeamProjectCollection);

            ReportServerUrl = proxy.ReportServiceUrl;
            ReportFolder = GetReportFolder(regService);

            return proxy;
        }

        public void SaveChanges()
        {
            var args = new WorkItemIssueEventArgs();

            foreach (var item in BacklogChildren)
            {
                if (item.Backlog.IsDirty)
                {
                    var validationErrors = item.Backlog.Validate();
                    if (validationErrors.Count > 0)
                        args.WorkItemIssues.Add(new WorkItemIssue { WorkItem = item.Backlog });
                    else
                    {
                        try
                        {
                            item.Backlog.Save();
                        }
                        catch(Exception ex)
                        {
                            Logger.LogException(ex);
                            args.WorkItemIssues.Add(new WorkItemIssue { WorkItem = item.Backlog });
                        }
                    }
                }
                foreach (var workItem in item.Children)
                {
                    if (workItem.IsDirty)
                    {
                        var validationErrors = workItem.Validate();
                        if (validationErrors.Count > 0)
                            args.WorkItemIssues.Add(new WorkItemIssue { WorkItem = workItem });
                        else
                        {
                            try
                            {
                                workItem.Save();
                            }
                            catch(Exception ex)
                            {
                                Logger.LogException(ex);
                                args.WorkItemIssues.Add(new WorkItemIssue { WorkItem = workItem });
                            }
                        }
                    }
                }
            }
            OnWorkItemIssuesOccurred(args);
        }
        public void Disconnect()
        {
            TeamProject = null;
            TeamProjectCollection = null;
            WorkItemLinkInfos = new WorkItemLinkInfo[0];
            if (AllChildren != null) AllChildren.Clear();
            if (BacklogChildren.Count > 0) 
                ApplyChildrenChangesOnUIThread(new List<BacklogChildren>());
            if (BackLogItems != null) BackLogItems.Clear();
            LoginData = new LoginData();
            ReportFolder = null;
            ReportServerUrl = null;
            WorkItems = null;
            OnDisconnected();
        }

        public IBacklogChildren GetBacklogFromWorkItem(WorkItem workItem)
        {
            // go through all backlogs
            foreach (var backlog in BacklogChildren)
            {
                // check backlog WI
                if (backlog.Backlog == workItem)
                    return backlog;

                // go throug all backlog children WIs
                foreach (var backlogChildrenWI in backlog.Children)
                {
                    // check backlog children WI
                    if (backlogChildrenWI == workItem)
                        return backlog;
                }
            }

            return null;
        }
        public bool HandleConnectionException(Exception ex)
        {
            var connectionException = ex as ConnectionException;
            if (connectionException != null)
            {
                OnConnectionFailed(connectionException.Message);
                return true;
            }

            return false;
        }

        public WorkItem CreateNewLinkedWorkItem(WorkItem parent,
                                            WorkItemLinkTypeEnd linkType,
                                            WorkItemType workItemType,
                                            string title,
                                            string comment)
        {

            WorkItemType wit = TeamProject.WorkItemTypes[workItemType.Name];

            // Create the work item. 
            WorkItem workItem = new WorkItem(wit);
            workItem.Title = title;
            workItem.Description = comment;
            workItem.AreaPath = parent.AreaPath;
            workItem.IterationPath = parent.IterationPath;  

            // Return the newly created work item
            return workItem;
        }

        public void SaveLinkedItem(WorkItem parent, WorkItem item, WorkItemLinkTypeEnd linkType)
        {
            item.Save();
            // Add link to parent item and save it
            parent.Links.Add(new RelatedLink(linkType, item.Id));
            parent.Save();
        }

        public void ApplyConfigurationToLink(WorkItem item, WorkItem parent)
        {
            if (SyncService != null)
            {
                if (!SyncService.Value.CheckAccess())
                {
                    SyncService.Value.Invoke(new Action<WorkItem, WorkItem>(ApplyConfigurationToLink), item, parent);
                    return;
                }
            }
            foreach (var bcklog in BacklogChildren)
            {
                if (bcklog.Backlog.Id == parent.Id)
                {
                    bcklog.Children.Add(item);
                    FlushWindowsMessageQueue(true);
                    break;
                }
            }
            AllChildren.Add(item);
        }

        #endregion

        #region Events
        public event EventHandler<ErrorEventArgs> ConnectionFailed;
        public event EventHandler<ErrorEventArgs> TfsCommunicationError;
        public event EventHandler<LoginDataEventArgs> RequestLoginData;
        public event EventHandler<WorkItemIssueEventArgs> WorkItemIssuesOccurred;
        public event EventHandler Connected;
        public event EventHandler Disconnected;
        public event EventHandler<RequestEventArgs> RequestSaveWorkItems;
        public event EventHandler ConfigurationApplied;

        public void OnTfsCommunicationError(string message)
        {
            if (TfsCommunicationError != null)
                TfsCommunicationError(this, new ErrorEventArgs {ErrorMessage = message});
        }
        
        protected virtual void OnConnectionFailed(string message)
        {
            if (ConnectionFailed != null)
                ConnectionFailed(this, new ErrorEventArgs {ErrorMessage = message});
        }
        protected virtual void OnRequestLoginData(LoginDataEventArgs args)
        {
            if (RequestLoginData != null)
            {
                RequestLoginData(this, args);
            }
        }
        protected virtual void OnConnected()
        {
            if (Connected != null)
            {
                Connected(this, EventArgs.Empty);
            }
        }
        protected virtual void OnRequestSaveWorkItems(RequestEventArgs args)
        {
            if (RequestSaveWorkItems != null)
            {
                // If any of the event handlers either cancels the request or handels the request we do not need to call the other handlers.
                foreach (EventHandler<RequestEventArgs> handler in RequestSaveWorkItems.GetInvocationList())
                {
                    handler(this, args);
                    if (args.Cancel) return;
                    if (args.Handled) return;
                }
            }
        }
        protected virtual void OnWorkItemIssuesOccurred(WorkItemIssueEventArgs args)
        {
            if ((WorkItemIssuesOccurred != null) && (args != null))
                WorkItemIssuesOccurred(this, args);
        }
        protected virtual void OnDisconnected ()
        {
            if (Disconnected != null)
            {
                Disconnected(this, EventArgs.Empty);
            }
        }
        protected virtual void OnConfigurationApplied ()
        {
            if (ConfigurationApplied != null)
            {
                ConfigurationApplied(this, EventArgs.Empty);
            }
        }
        #endregion

        #region Private Functions
        private void GetReportsFromServer(List<IHierarchicalItem> items, ReportingServiceProxy proxy)
        {
            try
            {
                items.AddRange(SaveGetCatalogItems(proxy).Select(item => new HierarchicalItem(proxy, item)));
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                OnTfsCommunicationError(ex.Message);
            }
            
        }
        private IEnumerable<CatalogItem> SaveGetCatalogItems(ReportingServiceProxy proxy)
        {
            try
            {
                return proxy.ListChildren(ReportFolder, false);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                OnTfsCommunicationError(ex.Message);
            }
            return new CatalogItem[0];
        }
        private static bool IsValidLinkInfo(WorkItemLinkInfo linkInfo, WorkItemLinkType linkType)
        {
            if (linkType == null) return false;

            // If the link id in the link info is 0 we handle a root item --> ann accepted sceanrio
            if (linkInfo.LinkTypeId == 0) return true;
            // If the link id from the link info matches the forward end link id of the given link type than we have a link that fits
            if (linkInfo.LinkTypeId == linkType.ForwardEnd.Id) return true;
            // IN all other cases the link type being configured does not fit to the link info being returned by a query
            return false;
        }
        private void PrepareTransitionsMap(IConfiguration configuration)
        {
            WorkItemTransitions = new Dictionary<string, List<Transition>>();
            
            if (configuration == null) return;
            foreach (var childItem in configuration.ChildItems)
            {
                List<Transition> transitions = GetTransitions(childItem);
                WorkItemTransitions[childItem] = transitions;
                
            }
            foreach (var backlogItem in configuration.BacklogItems)
            {
                List<Transition> transitions = GetTransitions(backlogItem);
                WorkItemTransitions[backlogItem] = transitions;
            }
        }

        private List<Transition> GetTransitions(string childItem)
        {
            var transitions = new List<Transition>();
            var workItemType = TeamProject.WorkItemTypes[childItem];
            // get Work Item type definition
            XmlDocument witd = workItemType.Export(true);
            // retrieve the transitions node
            XmlNode transitionsNode = witd.SelectSingleNode("descendant::TRANSITIONS");
            // for each transition definition (== possible next allowed state)
            if (transitionsNode != null)
            {
                var nodes = transitionsNode.SelectNodes("TRANSITION");
                if (nodes != null)
                {
                    foreach (XmlNode transitionNode in nodes)
                    {
                        string fromState = transitionNode.Attributes.GetNamedItem("from").Value;
                        string toState = transitionNode.Attributes.GetNamedItem("to").Value;
                        var reasons = new List<string>();
                        // retrieve the reasons node
                        XmlNode reasonsNode = transitionNode.SelectSingleNode("REASONS");
                        // for each state change reason 
                        foreach (XmlNode reason in reasonsNode.ChildNodes)
                        {
                            // show the reason
                            reasons.Add(reason.Attributes[0].Value);
                        }
                        var transition = new Transition {FromState = fromState, ToState = toState, Reasons = reasons.ToArray()};
                        transitions.Add(transition);
                    }
                }
            }
            return transitions;
        }

        private void BuildChildItemsList(IConfiguration configuration)
        {
            var backlogChildrenList = new List<BacklogChildren>();
            AllChildren = new List<WorkItem>();

            //keep a hash with the backlog items ids. It implies an additional iteration through the collection,
            //but we make up for this when searching for parents in constant time rather than linear
            HashSet<int> backLogIds = new HashSet<int>();
            BackLogItems.ForEach(wi => backLogIds.Add(wi.Id));

            // Iterate over all backlog items and get the list of child items.
            // Keep track of the relation between a backlog item and its child item via the BacklogChildren class.
            foreach (var backLogItem in BackLogItems)
            {
                var currentItem = backLogItem;
                // Get all the work items that the current item points to
                var childInfos = WorkItemLinkInfos.Where(info => info.SourceId == currentItem.Id).ToList();
                // Get the Ids of tht work items
                var childIds = (from WorkItemLinkInfo info in childInfos select info.TargetId).ToList();
                // Get the work items for that ids but only if they are not already in the backlog items list and if they match the child item setting

                // Note: Uncomment the following line and comment the line after in order to hide backlog items from the child area
                //var children = WorkItems.Cast<WorkItem>().Where(item => childIds.Contains(item.Id) && !backLogIds.Contains(item.Id) && configuration.ChildItems.Contains(item.Type.Name)).ToList();
                var children = WorkItems.Cast<WorkItem>().Where(item => childIds.Contains(item.Id) && configuration.ChildItems.Contains(item.Type.Name)).ToList();

                AllChildren.AddRange(children);
                var backlogChildren = new BacklogChildren(backLogItem, children, configuration.States);
                backlogChildrenList.Add(backlogChildren);

            }

            ApplyChildrenChangesOnUIThread(backlogChildrenList);
        }
        private void ApplyChildrenChangesOnUIThread(List<BacklogChildren> backlogChildrenList)
        {
            // The UI binds to BacklogChildren and that's why we must switch to the UI thread before changing the list
            if (SyncService != null)
            {
                if (!SyncService.Value.CheckAccess())
                {
                    SyncService.Value.Invoke(new Action<List<BacklogChildren>>(ApplyChildrenChangesOnUIThread), backlogChildrenList);
                    return;
                }
            }
            BacklogChildren.Clear();
            foreach (var children in backlogChildrenList)
            {
                BacklogChildren.Add(children);               
                FlushWindowsMessageQueue(true);
            }
        }

        /// <summary>
        /// Flushes the windows message queue by calling an empty function.
        /// Use this function when you want to update the UI (e.g. after adding a status message)
        /// </summary>
        /// <param name="updateBindings">if set to <c>true</c> [update bindings].</param>
        private void FlushWindowsMessageQueue(bool updateBindings)
        {
            if (Application.Current == null)
                return; // application is closing
            if(updateBindings)
                //announce that the configuration has been applied after each row, so that the control rebinds
                Application.Current.Dispatcher.Invoke(new Action(OnConfigurationApplied), DispatcherPriority.Background);
            else
                Application.Current.Dispatcher.Invoke(new Action(DummyFunction), DispatcherPriority.Background);
        }

        private void DummyFunction()
        {
            
        }

        private void BuildBacklogItemsList(IConfiguration configuration)
        {
            List<String> backlogItemTypes = configuration.BacklogItems;
            if (!String.IsNullOrEmpty(configuration.SortFieldName))
            {
                if (configuration.SortDirection == SortDirection.Ascending)
                {
                    BackLogItems = WorkItems.Cast<WorkItem>().Where(item => backlogItemTypes.Contains(item.Type.Name) && item.Fields.Contains(configuration.SortFieldName)).OrderBy(b => b.Fields[configuration.SortFieldName].Value).ToList();
                }
                else
                {
                    BackLogItems = WorkItems.Cast<WorkItem>().Where(item => backlogItemTypes.Contains(item.Type.Name) && item.Fields.Contains(configuration.SortFieldName)).OrderByDescending(b => b.Fields[configuration.SortFieldName].Value).ToList();
                }
            }
            else
            {
                BackLogItems = WorkItems.Cast<WorkItem>().Where(item => backlogItemTypes.Contains(item.Type.Name)).ToList();
            }
        }
        private Query GetQuery(QueryItem queryItem)
        {
            var definition = queryItem as QueryDefinition;
            if (definition != null)
            {
                var wiqlContext = new Dictionary<string, object>
                                      {
                                          {"project", TeamProject.Name},
                                          {"me", TeamProjectCollection.AuthorizedIdentity.DisplayName},
                                          {"today", DateTime.Now.Date}
                                      };
                var query = new Query(TeamProject.Store, definition.QueryText, wiqlContext);
                return query;
            }
            return null;
        }
        private string GetReportFolder(IRegistration regService)
        {
            try
            {
                if (TeamProjectCollection.ConfigurationServer == null)
                    return new Uri(String.Concat("/", TeamProject.Name), UriKind.Relative).ToString();

                var entries = regService.GetRegistrationEntries("TeamProjects");

                Uri uri = null;
                var match = String.Concat(TeamProject.Name, ":");
                const StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;

                foreach (var si in entries[0].ServiceInterfaces)
                {
                    if (si.Name.StartsWith(match, comparison) && !String.IsNullOrEmpty(si.Url))
                    {
                        var serviceName = si.Name.Substring(match.Length).Trim();
                        if (serviceName.Equals("ReportFolder", comparison))
                        {
                            if (Uri.TryCreate(si.Url, UriKind.Relative, out uri))
                                return uri.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                OnTfsCommunicationError(ex.Message);
            }
            return null;
        }
        private string GetReportServiceUrl(IRegistration regService)
        {
            try
            {
                var reportEntries = regService.GetRegistrationEntries("Reports");
                return (from reportEntry in reportEntries
                        select
                            reportEntry.ServiceInterfaces.FirstOrDefault(
                                serviceEntry => serviceEntry.Name == ServiceInterfaces.ReportWebServiceUrl)
                        into serviceInterface
                        where serviceInterface != null
                        select serviceInterface.Url).FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                OnTfsCommunicationError(ex.Message);
            }
            return null;
        }
        private QueryDefinition SaveGetQueryDefinition(WorkItemStore store, Guid queryId)
        {
            // Issue: store.GetQueryDefinition(id) fails in  some cases. Use an alterantive approach
            if (TeamProject == null) return null;
            try
            {
                return FindQueryInHierarchy(TeamProject.QueryHierarchy, queryId);
            }
            catch (Exception ex)
            {
                Logger.LogException(ex);
                // TODO: Notify the usert about the issue we have with retrieving the query
                return null;
            }
        }

        private QueryDefinition FindQueryInHierarchy(QueryFolder queryFolder, Guid queryId)
        {
            foreach (QueryItem item  in queryFolder)
            {
                var queryDefinition = item as QueryDefinition;
                var childFolder = item as QueryFolder;
                // If we have a query defintion check it's id and return it if it fits
                if ((queryDefinition != null) && (queryDefinition.Id == queryId)) return queryDefinition;
                // Otherwise if we have a folder traverse the folder to check it's children
                if (childFolder != null)
                {
                    var foundItem = FindQueryInHierarchy(childFolder, queryId);
                    if (foundItem != null) return foundItem;
                }
            }
            return null;
        }

        private bool VerifySaveRequest()
        {
            if (WorkItems != null)
            {
                var workItemsHaveChanges = WorkItems.Cast<WorkItem>().Any(workItem => workItem.IsDirty);
                if (workItemsHaveChanges)
                {
                    var args = new RequestEventArgs();
                    OnRequestSaveWorkItems(args);
                    if (args.Cancel) return false;
                }
            }
            return true;
        }

        #endregion
    }
}
    