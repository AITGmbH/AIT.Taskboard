using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using WinForms = System.Windows.Forms;

namespace AIT.Taskboard.ViewModel
{
    public class ConfigurationViewModel : INotifyPropertyChanged
    {
        private static readonly Color DefaultStateColor = Colors.Blue;

        private List<string> _workItemStates;

        #region Constructors

        public ConfigurationViewModel()
        {
            BacklogItems = new List<string>();
            ChildItems = new List<string>();
            RestrictWorkItemTypeUsage = Properties.Settings.Default.RestrictWorkItemTypeUsage;
            StateColors = new List<Color>(Constants.PresetColors);
        }

        public ConfigurationViewModel(ITaskboardService taskboardService, IConfigurationService configurationService, IStatusService statusService, IDataObjectFactory factory, ILogger logger):this ()
        {
            TaskBoardService = taskboardService;
            StatusService = statusService;
            ConfigurationService = configurationService;
            DataObjectFactory = factory;
            Logger = logger;
        }

        #endregion

        #region Properties
        private ILogger Logger { get; set; }
        internal IConfiguration Configuration { get; set; }

        private ITaskboardService TaskBoardService { get; set; }
        private IStatusService StatusService { get; set; }
        private IConfigurationService ConfigurationService { get; set; }
        private IDataObjectFactory DataObjectFactory { get; set; }

        public WorkItemTypeCollection WorkItemTypes { get; private set; }
        public ObservableCollection<ICustomState> CustomStates { get; private set; }
        public ICustomState CurrentCustomState { get; set; }
        public List<string> WorkItemStates 
        {
            get { return _workItemStates; }
            private set
            {
                _workItemStates = value;
                OnPropertyChanged("WorkItemStates");
            } 
        }

        public QueryHierarchy QueryHierarchy { get; private set; }

        private QueryFolder _copiedQueryHierarchy;
        /// <summary>
        /// Gets or sets the copied query hierarchy. We need this object for binding it to the TreeView.
        /// If we use the QueryHierarchy object, the TreeView will not display the updated queries.
        /// </summary>
        /// <value>The copied query hierarchy.</value>
        public QueryFolder CopiedQueryHierarchy
        {
            get
            {
                return _copiedQueryHierarchy;
            }
            set
            {
                _copiedQueryHierarchy = new QueryFolder(value.Name);
                CopyQueryHierarchy(value, _copiedQueryHierarchy);
                OnPropertyChanged("CopiedQueryHierarchy");
            }
        }

        public List<FieldDefinition> PossibleSortFields { get; private set; }
        public List<SortDirection> SortDirections { get; private set; }
        public List<FieldDefinition> PossibleSummaryFields { get; private set; }
        public FieldDefinition CurrentRowSummaryField { get; set; }
        public FieldDefinition CurrentColumnSummaryField { get; set; }
        public FieldDefinition CurrentSortField { get; set; }
        public WorkItemLinkType CurrentLinkType { get; set; }
        public SortDirection CurrentSortDirection { get; set; }
        /// <summary>
        /// Gets or sets current value of AutoRefreshDelay - used in binding.
        /// </summary>
        public int CurrentAutoRefreshDelay { get; set; }
        public List<string> BacklogItems { get; private set; }
        public List<string> ChildItems { get; private set; }
        public List<Color> StateColors { get; private set; }
        public WorkItemLinkTypeCollection LinkTypes { get; private set; }
        private List<WorkItemLinkTypeEnd> _linkTypeEnds;
        public IEnumerable<WorkItemLinkTypeEnd> LinkTypeEnds
        {
            get
            {
                if (LinkTypes == null)
                    return null;
                if (_linkTypeEnds == null)
                {
                    _linkTypeEnds = new List<WorkItemLinkTypeEnd>();
                    _linkTypeEnds.AddRange(LinkTypes.LinkTypeEnds);
                    _linkTypeEnds.RemoveAll(wi => wi.ImmutableName == "System.LinkTypes.Hierarchy-Reverse");//remove parent link type, we only want to add children
                    _linkTypeEnds.Sort((x, y) => String.Compare(x.Name, y.Name));
                }
                return _linkTypeEnds;
            }
        }
        public string ConfigurationData { get; set; }
        public bool RestrictWorkItemTypeUsage { get; set; }
        public string TeamProjectName { get; set; }
        public Uri TeamProjectCollectionUri { get; set; }
        public bool HideColumnSummaryFieldname { get; set; }
        public WorkItemSize WorkItemSize { get; set; }
        public bool HideReportViewer { get; set; }
        #endregion

        #region Methods

        #region QueryItem
        private QueryItem _queryItem;
        public QueryItem QueryItem
        {
            get { return _queryItem; }
            set { _queryItem = value; }
        }

        public void SetQueryItem(object queryItem)
        {
            var newQueryItem = queryItem as QueryItem;
            if (newQueryItem != null)
            {
                // Remember the selected query.
                QueryItem = newQueryItem;
                OnPropertyChanged("QueryItem");
                // Configure the fields available for Summary and sorting
                PossibleSummaryFields = TaskBoardService.GetPossibleSummaryFields(newQueryItem);
                PossibleSortFields = TaskBoardService.GetPossibleSortFields(newQueryItem);
                OnPropertyChanged("PossibleSummaryFields");
                OnPropertyChanged("PossibleSortFields");
            }
        }

        /// <summary>
        /// Gets the query item from the original collection that matches the given query item.
        /// </summary>
        /// <param name="queryItem">The query item.</param>
        /// <returns></returns>
        public QueryItem GetQueryItem(object queryItem)
        {
            return Find(queryItem as QueryItem, QueryHierarchy);
        }

        /// <summary>
        /// Finds the specified query item recursively.
        /// </summary>
        /// <param name="queryItem">The query item for which to search.</param>
        /// <param name="folder">The folder in which to search.</param>
        /// <returns></returns>
        private QueryItem Find(QueryItem queryItem, QueryFolder folder)
        {
            foreach (QueryItem item in folder)
            {
                if (item.Path == queryItem.Path)
                    return item;
                if (queryItem.Path.StartsWith(item.Path) && item is QueryFolder)
                    return Find(queryItem, item as QueryFolder);
            }
            return null;
        }
        #endregion

        public void Commit(bool forceSaveToFile)
        {
            if (!IsConfigurationLoaded) return;
            string configurationData = ConfigurationData;
            if (!ConfigurationService.UseMemoryFile && 
                (string.IsNullOrEmpty(configurationData) || !File.Exists(configurationData))) 
                return;
            // Now it's time to keep all the settings in the taskboards configuration
            IConfiguration configuration = ConfigurationService.ReadTaskboardConfiguration(configurationData);
            configuration.SortFieldName = CurrentSortField != null ? CurrentSortField.Name : null;
            configuration.SortDirection = CurrentSortDirection;
            configuration.AutoRefreshDelay = CurrentAutoRefreshDelay;
            configuration.RowSummaryFieldName = CurrentRowSummaryField!= null ? CurrentRowSummaryField.Name: null;
            configuration.ColumnSummaryFieldName = CurrentColumnSummaryField != null ? CurrentColumnSummaryField.Name : null;
            EnsureItems(configuration.BacklogItems, BacklogItems);
            EnsureItems(configuration.ChildItems, ChildItems);
            configuration.States.Clear();
            foreach (var state in CustomStates)
            {
                configuration.States.Add(state.Clone() as ICustomState);
            }
            configuration.QueryId = QueryItem != null ? QueryItem.Id : Guid.Empty;
            configuration.LinkType = CurrentLinkType != null ? CurrentLinkType.ReferenceName : null;
            // Store the current login information. This is required so that double clicking the file will open the right project from the right server
            configuration.TeamProject = TeamProjectName;
            configuration.TeamProjectCollection = TeamProjectCollectionUri.ToString();
            configuration.HideColumnSummaryFieldName = HideColumnSummaryFieldname;
            configuration.WorkItemSize = WorkItemSize;
            configuration.HideReportViewer = HideReportViewer;
            if (forceSaveToFile)
                ConfigurationService.UseMemoryFile = false;
            // Persist the configuration
            ConfigurationService.SaveTaskboardConfiguration(configurationData, configuration);
            if (!ConfigurationService.UseMemoryFile)
            {
                MruFileManager fileManager = new MruFileManager();
                fileManager.AddFile(ConfigurationData);
            }
            //TODO: is there any reason to re-apply the configuration?
            //TaskBoardService.ApplyConfiguration(configuration);
        }
        
        private void EnsureItems(List<string> listToUpdated, List<string> listWithNewState)
        {
            // Remove all items from listToUpdate that are not contained in listWithNewState
            List<string> itemsToRemove = listToUpdated.Where(item => !listWithNewState.Contains(item)).ToList();
            List<string> itemsToAdd = listWithNewState.Where(item => !listToUpdated.Contains(item)).ToList();
            foreach (string item in itemsToRemove)
            {
                listToUpdated.Remove(item);
            }
            listToUpdated.AddRange(itemsToAdd);
        }

        public void LoadConfiguration()
        {
            string configurationData = ConfigurationData;
            if (ConfigurationService.UseMemoryFile)
            {
                IConfiguration config = ConfigurationService.ReadTaskboardConfiguration(null);
                LoadConfiguration(config);
                return;
            }
            if (!string.IsNullOrEmpty(configurationData))
            {
                IConfiguration config = ConfigurationService.ReadTaskboardConfiguration(configurationData);
                LoadConfiguration(config);
            }
        }

        public void LoadConfiguration(IConfiguration config)
        {
            Configuration = config;
            LinkTypes = TaskBoardService.GetLinkTypes();
            OnPropertyChanged("LinkTypes");

            if (Configuration.QueryId != Guid.Empty)
            {
                try
                {
                    SetQueryItem(TaskBoardService.GetQueryItem(Configuration.QueryId));
                    CurrentSortField = PossibleSortFields.SingleOrDefault(sf => sf.Name == Configuration.SortFieldName);
                    CurrentRowSummaryField = PossibleSummaryFields.SingleOrDefault(sf => sf.Name == Configuration.RowSummaryFieldName);
                    CurrentColumnSummaryField = PossibleSummaryFields.SingleOrDefault(sf => sf.Name == Configuration.ColumnSummaryFieldName);
                    OnPropertyChanged("CurrentSortField");
                    OnPropertyChanged("CurrentRowSummaryField");
                    OnPropertyChanged("CurrentColumnSummaryField");
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    var statusItem = StatusService.EnqueueStatusItem("InvalidQueryId");
                    statusItem.Message = "The configured query is invalid. Please edit the configuration.";
                }
            }

            CurrentSortDirection = Configuration.SortDirection;
            HideColumnSummaryFieldname = Configuration.HideColumnSummaryFieldName;
            WorkItemSize = Configuration.WorkItemSize;
            HideReportViewer = Configuration.HideReportViewer;
            CurrentLinkType = LinkTypes.FirstOrDefault(linkType => linkType.ReferenceName == Configuration.LinkType);
            CurrentAutoRefreshDelay = Configuration.AutoRefreshDelay;
            OnPropertyChanged("CurrentSortDirection");
            OnPropertyChanged("HideColumnSummaryFieldname");
            OnPropertyChanged("WorkItemSize");
            OnPropertyChanged("CurrentLinkType");
            OnPropertyChanged("CurrentAutoRefreshDelay");
            QueryHierarchy = TaskBoardService.GetWorkItemQueries(true);
            var clonedStates = (from state in Configuration.States select state.Clone() as ICustomState).ToList();
            CustomStates = new ObservableCollection<ICustomState>(clonedStates);
            OnPropertyChanged("CustomStates");

            if (Configuration.ChildItems != null)
                ChildItems = new List<string>(Configuration.ChildItems);
            if (Configuration.BacklogItems != null)
                BacklogItems = new List<string>(Configuration.BacklogItems);

            LoadWorkItemTypes();
            SortDirections = new List<SortDirection>((IEnumerable<SortDirection>)Enum.GetValues(typeof(SortDirection)));
            if (!String.IsNullOrEmpty(Configuration.TeamProject))
                TeamProjectName = Configuration.TeamProject;
            if (!String.IsNullOrEmpty(Configuration.TeamProjectCollection))
                TeamProjectCollectionUri = new Uri(Configuration.TeamProjectCollection);

            TaskBoardService.ApplyConfiguration(Configuration);
            OnPropertyChanged("CustomStates");
            IsConfigurationLoaded = true;
        }

        public bool IsConfigurationLoaded { get; set; }

        public void Reset()
        {
            // Any changes being made must be reverted.We do this be just reloading the configuration from file.
            LoadConfiguration();
            StatusService.DequeueStatusItem("ApplyConfiguration");
        }

        public void LoadWorkItemStates()
        {
            WorkItemStates = new List<string>();
            if (WorkItemTypes != null)
                foreach (WorkItemType workItemType in WorkItemTypes)
                {
                    FieldDefinition stateFieldDefintion = workItemType.FieldDefinitions[CoreField.State];
                    foreach (string allowedValue in stateFieldDefintion.AllowedValues)
                    {
                        if (!WorkItemStates.Contains(allowedValue) && ChildItems.Contains(workItemType.Name))
                            WorkItemStates.Add(allowedValue);
                    }
                }
            OnPropertyChanged("WorkItemStates");
        }

        private void LoadWorkItemTypes()
        {
            WorkItemTypeCollection typesFromServer = TaskBoardService.GetWorkItemTypes();
            WorkItemTypes = typesFromServer;
            OnPropertyChanged("WorkItemTypes");
            LoadWorkItemStates();
        }

        public void AddCustomState(string name)
        {
            ICustomState customState = DataObjectFactory.CreateObject<ICustomState> ();
            customState.Name = name;
            customState.Color = DefaultStateColor;
            CustomStates.Add(customState);
        }
        
        public void RemoveCurrentCustomState()
        {
            CurrentCustomState.WorkItemStates.Clear();
            CustomStates.Remove(CurrentCustomState);
            CurrentCustomState = null;
            OnPropertyChanged("CustomStates");
        }

        public void RefreshConfiguration()
        {
            TaskBoardService.ApplyConfiguration(Configuration);
        }

        public void RefreshSummaries()
        {
            OnPropertyChanged("CurrentColumnSummaryField");
            OnPropertyChanged("CurrentRowSummaryField");
        }

        public void RefreshQueryItems()
        {
            if (TaskBoardService != null)
            {
                QueryHierarchy = TaskBoardService.GetWorkItemQueries(true);
                //we need to copy the hierarchy to another object and bind the TreeView to that object
                //otherwise, the TreeView will show the old hierarchy even if it was updated
                CopiedQueryHierarchy = QueryHierarchy;
            }
        }

        /// <summary>
        /// Copies the collection to the destination foloder.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <param name="destinationFolder">The destination folder.</param>
        private void CopyQueryHierarchy(IEnumerable<QueryItem> collection, QueryFolder destinationFolder)
        {
            foreach (QueryItem item in collection)
            {
                if (item is QueryFolder)
                {
                    QueryFolder copiedFolder = new QueryFolder(item.Name, destinationFolder);
                    CopyQueryHierarchy(item as IEnumerable<QueryItem>, copiedFolder);
                }
                if (item is QueryDefinition)
                {
                    QueryDefinition copiedDefinition = new QueryDefinition(item.Name,
                        ((QueryDefinition)item).QueryText, destinationFolder);
                }
            }
        }

        public void SelectTeamProject()
        {
            var picker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            
            if (TeamProjectCollectionUri != null)
            {
                try
                {
                    picker.SelectedTeamProjectCollection = new TfsTeamProjectCollection(TeamProjectCollectionUri);
                }
                catch(Exception ex)
                {
                    Logger.LogException(ex);
                }
            }
            // TODO: Set Team Project according to the login data
            WinForms.DialogResult result = picker.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                TaskBoardService.LoginData.TeamProjectName = picker.SelectedProjects[0].Name;
                TaskBoardService.LoginData.TeamProjectCollectionUri = picker.SelectedTeamProjectCollection.Uri;
                TaskBoardService.Connect();
                LoadConfiguration(ConfigurationService.GetDefaultConfiguration());
                
                RefreshQueryItems();
                SetQueryItem(CopiedQueryHierarchy);
                
                TeamProjectCollectionUri = picker.SelectedTeamProjectCollection.Uri;
                TeamProjectName = picker.SelectedProjects[0].Name;
                
                OnPropertyChanged("TeamProjectName");
            }
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

       
    }
}
