using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using ErrorEventArgs = AIT.Taskboard.Interface.ErrorEventArgs;
using WinForms = System.Windows.Forms;

namespace AIT.Taskboard.ViewModel
{
    [Export]
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        #region Constants

        public const double MinZoomFactor = 0.5;
        public const double MaxZoomFactor = 1.5;
        public const double ZoomStep = 0.1;

        #endregion

        #region Fields

        private WorkItem _selectedWorkItem;
        private bool _showEditBar;
        private bool _showReportViewerBar;
        private IList<string> _assignedToFilterList;
        private string _assignedToFilterSelectedItem;
        private string _childrenFilterSelectedItem;
        private string _selectedAreaPath;
        private string _selectedIterationPath;
        private string _workItemUserFilter;
        private ReportViewModel _reportViewModel;
        private double _zoomFactor = 1;
        private ObservableCollection<IBacklogChildren> _backlogChildren;
        private string _configurationFileName;
        private StatusViewModel _statusViewModel;
        private bool _showPopupWindow;
        private DateTime _endTime;
        private int _remainingSeconds;
        private bool _isConnecting;
        private List<string> _areaPaths;
        private List<string> _iterationPaths;
        private WorkItemSize _selectedWorkItemSize;
       
        #endregion

        #region Constructor

        [ImportingConstructor]
        public ApplicationViewModel(ITaskboardService taskboardService, IStatusService statusService)
        {
            TaskboardService = taskboardService;
            //TaskboardService.Connected += HandleTaskboardServiceConnected;
            TaskboardService.ConnectionFailed += HandleConnectionFailed;
            TaskboardService.TfsCommunicationError += HandleTfsCommunicationError;
            TaskboardService.ConfigurationApplied += HandleConfigurationApplied;
            MruFileManager = new MruFileManager();
            StatusViewModel = new StatusViewModel(TaskboardService, statusService, null, WorkItemTemplateProvider, Logger);
            ShowPopupWindow = false;
            ShowEditbar = true;
        }

        #endregion

        #region Properties

        private MruFileManager MruFileManager { get; set; }

        [Import]
        private  IStatusService StatusService { get; set; }

        [Import]
        private IDataObjectFactory Factory { get; set; }

        [Import]
        private ISyncService SyncService { get; set; }

        [Import]
        public IConfigurationService ConfigurationService { get; set; }

        [Import]
        public IWorkItemTemplateProvider WorkItemTemplateProvider { get; private set; }

       [Import]
        public ILogger Logger { get; set; }

        public string ConfigurationFileName
        {
            get { return _configurationFileName; }
            set
            {
                if (ConfigurationFileName != value)
                {
                    _configurationFileName = value;
                    OnPropertyChanged("ConfigurationFileName");
                }
            }
        }

        public bool IsTrialVersion
        {
            //return the license service
            get
            {
                return false;
            }
        }


        public StatusViewModel StatusViewModel 
        {
            get { return _statusViewModel; }
            set 
            { 
                _statusViewModel = value;
                OnPropertyChanged("StatusViewModel");
            }
        }

        public ConfigurationViewModel ConfigurationViewModel { get; set; }

        public LinkedWorkItemViewModel LinkedWorkItemViewModel { get; set; }
        
        public ITaskboardService TaskboardService { get; set; }

        public ReportViewModel ReportViewModel
        {
            get { return _reportViewModel; }
            set
            {
                if (_reportViewModel != value)
                {
                    _reportViewModel = value;
                    OnPropertyChanged("ReportViewModel");
                }
            }
        }

        public WorkItem SelectedWorkItem
        {
            get { return _selectedWorkItem; }
            set
            {
                _selectedWorkItem = value;
                OnPropertyChanged("SelectedWorkItem");
                OnPropertyChanged("SelectedWorkItemColor");
                OnPropertyChanged("ShowEditbar");
                OnPropertyChanged("WorkItemSelected");
            }
        }

        public bool WorkItemSelected
        {
            get
            {
                return _selectedWorkItem != null;
            }
        }

        public bool ShowEditbar
        {
            get { return _selectedWorkItem != null && _showEditBar; }
            set
            {
                _showEditBar = value;
                OnPropertyChanged("SelectedWorkItemColor");
                OnPropertyChanged("ShowEditbar");
            }
        }

        public bool ShowReportViewBarChangedFromUI { get; set; }

        public bool CreateNewConfigurationFromExisting { get; set; }

        public bool ShowReportViewerBar
        {
            get { return _showReportViewerBar; }
            set
            {
                ConfigurationViewModel.HideReportViewer = _showReportViewerBar = value;

                if (ReportViewModel != null)
                    ReportViewModel.HideReports = value;

                OnPropertyChanged("ShowReportViewerBar");
            }
        }

        public bool AllowSmallWorkItems
        {
            get
            {
                return WorkItemTemplateProvider.IsSizeDefined(Interface.WorkItemSize.Small);
            }
        }

        public bool AllowLargeWorkItems
        {
            get
            {
                return WorkItemTemplateProvider.IsSizeDefined(Interface.WorkItemSize.Large);
            }
        }

        public Size WorkItemSize
        {
            get
            {
                return WorkItemTemplateProvider.GetWorkItemSize(SelectedWorkItemSize);
            }
        }

        public double WorkItemWidth
        {
            get
            {
                return WorkItemSize.Width;
            }
        }

        public double WorkItemHeight
        {
            get
            {
                return WorkItemSize.Height;
            }
        }

        public WorkItemSize SelectedWorkItemSize
        {
            get
            {
                if (ConfigurationViewModel == null)
                    return Interface.WorkItemSize.Medium;
                return ConfigurationViewModel.WorkItemSize;
            }
            set
            {
                if (ConfigurationViewModel == null)
                    return;
                ConfigurationViewModel.WorkItemSize = value;
                ZoomFactor = 1;
                OnPropertyChanged("WorkItemSize");
                OnPropertyChanged("WorkItemWidth");
                OnPropertyChanged("WorkItemHeight");
                OnPropertyChanged("SelectedWorkItemSize");
            }
        }

        #region Filters
        public IList<string> AssignedToFilterList
        {
            get
            {
                if (_assignedToFilterList == null && TaskboardService != null && TaskboardService.BacklogChildren != null)
                {
                    _assignedToFilterList = new List<string>(TaskboardService.BacklogChildren.Select(
                        b => (string)b.Backlog.Fields[CoreField.AssignedTo].Value).Union(
                            TaskboardService.BacklogChildren.SelectMany(
                                bc => bc.Children.Select(c => (string)c.Fields[CoreField.AssignedTo].Value))).Distinct());
                }
                return _assignedToFilterList;
            }
        }

        public string AssignedToFilterSelectedItem
        {
            get { return _assignedToFilterSelectedItem; }
            set 
            { 
                _assignedToFilterSelectedItem = value;
                OnPropertyChanged("AssignedToFilterSelectedItem");
                SelectedWorkItem = null;
            }
        }

        public string ChildrenFilterSelectedItem
        {
            get { return _childrenFilterSelectedItem; }
            set
            {
                _childrenFilterSelectedItem = value;
                OnPropertyChanged("ChildrenFilterSelectedItem");
                SelectedWorkItem = null;
            }
        }

        public List<string> AreaPaths
        {
            get
            {
                if (_areaPaths == null && TaskboardService != null)
                {
                    ICollection<Node> pathsAsTree = TaskboardService.GetAreaPaths();
                    if (pathsAsTree != null && pathsAsTree.Count > 0)
                    {
                        _areaPaths = new List<string>();
                        foreach (Node node in pathsAsTree)
                        {
                            AppendNodeAndChildren(node, ref _areaPaths);
                        }
                    }
                }
                return _areaPaths;
            }
        }

        public List<string> IterationPaths
        {
            get
            {
                if (_iterationPaths == null && TaskboardService != null)
                {
                    ICollection<Node> pathsAsTree = TaskboardService.GetIterationPaths();
                    if (pathsAsTree != null && pathsAsTree.Count > 0)
                    {
                        _iterationPaths = new List<string>();
                        foreach (Node node in pathsAsTree)
                        {
                            AppendNodeAndChildren(node, ref _iterationPaths);
                        }
                    }
                }
                return _iterationPaths;
            }
        }

        private void AppendNodeAndChildren(Node node, ref List<string> resultsList)
        {
            resultsList.Add(node.Path);
            if (node.HasChildNodes)
                foreach (Node childNode in node.ChildNodes)
                    AppendNodeAndChildren(childNode, ref resultsList);
        }

        public string SelectedAreaPath
        {
            get { return _selectedAreaPath; }
            set
            {
                _selectedAreaPath = value;
                OnPropertyChanged("SelectedAreaPath");
                SelectedWorkItem = null;
            }
        }

        public string SelectedIterationPath
        {
            get { return _selectedIterationPath; }
            set
            {
                _selectedIterationPath = value;
                OnPropertyChanged("SelectedIterationPath");
                SelectedWorkItem = null;
            }
        }

        public string WorkItemUserFilter
        {
            get { return _workItemUserFilter; }
            set
            {
                _workItemUserFilter = value;
                OnPropertyChanged("WorkItemUserFilter");
                SelectedWorkItem = null;
            }
        }
        #endregion

        public double ZoomFactor
        {
            get { return _zoomFactor; }
            set
            {
                if (ZoomFactor != value)
                {
                    if (value < MinZoomFactor && !AllowSmallerSize)
                    {
                        _zoomFactor = MinZoomFactor;
                    }
                    else if (value > MaxZoomFactor && !AllowLargerSize)
                    {
                        _zoomFactor = MaxZoomFactor;
                    }
                    else
                    {
                        _zoomFactor = value;
                    }
                    if (SelectedWorkItemSize != Interface.WorkItemSize.Small && _zoomFactor < 1 && WorkItemTemplateProvider.IsSizeDefined(SelectedWorkItemSize - 1))
                    {
                        double actualWidth = WorkItemWidth * ZoomFactor;
                        Size previousSize = WorkItemTemplateProvider.GetWorkItemSize((WorkItemSize)(SelectedWorkItemSize - 1));
                        if (actualWidth - previousSize.Width < WorkItemWidth - actualWidth)
                        {
                            SelectedWorkItemSize -= 1;
                            ZoomFactor = actualWidth / previousSize.Width;
                            return;
                        }
                    }
                    if (SelectedWorkItemSize != Interface.WorkItemSize.Large && _zoomFactor > 1 && WorkItemTemplateProvider.IsSizeDefined(SelectedWorkItemSize + 1))
                    {
                        double actualWidth = WorkItemWidth * ZoomFactor;
                        Size nextSize = WorkItemTemplateProvider.GetWorkItemSize((WorkItemSize)(SelectedWorkItemSize + 1));
                        if (nextSize.Width - actualWidth < actualWidth - WorkItemWidth )
                        {
                            SelectedWorkItemSize += 1;
                            ZoomFactor = actualWidth / nextSize.Width;
                            return;
                        }
                    }
                    OnPropertyChanged("ZoomFactor");
                }
            }
        }

        public bool AllowSmallerSize
        {
            get
            {
                if (SelectedWorkItemSize == Interface.WorkItemSize.Small)
                    return false;
                return WorkItemTemplateProvider.IsSizeDefined(SelectedWorkItemSize - 1);
            }
        }

        public bool AllowLargerSize
        {
            get
            {
                if (SelectedWorkItemSize == Interface.WorkItemSize.Large)
                    return false;
                return WorkItemTemplateProvider.IsSizeDefined(SelectedWorkItemSize + 1);
            }
        }

        public ObservableCollection<IBacklogChildren> BacklogChildren
        {
            get { return _backlogChildren ?? (_backlogChildren = new ObservableCollection<IBacklogChildren>()); }
        }

        public bool IsConnecting
        {
            get
            {
                return _isConnecting;
            }
            set
            {
                _isConnecting = value;
                OnPropertyChanged("IsConnecting");
            }
        }

        public bool IsConnected
        {
            get
            {
                return TaskboardService.IsConnected;
            }
        }

        public Color SelectedWorkItemColor
        {
            get
            {
                if (ShowEditbar)
                {
                    var customState = ConfigurationViewModel.CustomStates.SingleOrDefault(c => c.WorkItemStates.Contains(SelectedWorkItem.State));
                    if (customState != null)
                        return customState.Color;
                    else
                        return Colors.Gray;
                }
                return Colors.Transparent;
            }
        }

        /// <summary>
        /// Gets the custom states from the configuration. 
        /// We need to expose this property so that the taskboard control knows how to display the tasks according to their states.
        /// </summary>
        /// <value>The custom states.</value>
        public List<ICustomState> CustomStates
        {
            get
            {
                if ((ConfigurationViewModel == null) || (ConfigurationViewModel.Configuration == null))
                    return null;
                return ConfigurationViewModel.Configuration.States;
            }
        }

        public bool WorkItemsHaveChanges
        {
            get
            {
                foreach (var item in TaskboardService.BacklogChildren)
                {
                    if (item.Backlog.IsDirty)
                    {
                        return true;
                    }
                    foreach (var workItem in item.Children)
                    {
                        if (workItem.IsDirty)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public StringCollection MostRecentFiles { get { return MruFileManager.MostRecentlyUsedFiles; } }

        public string FeedbackUri
        {
            get
            {
                const string feedbackUriTemplate = "http://www.aitgmbh.de/index.php?id=121&L=1";
                // string feedbackUri = string.Format(feedbackUriTemplate, AboutModel.AssemblyName, AboutModel.AssemblyVersion);
                //feedbackUri = System.Web.HttpUtility.UrlDecode(feedbackUri);
                return feedbackUriTemplate;
            }
        }
        
        /// <summary>
        /// sets or gets Visibility of Popup Window used before AutoRefresh
        /// </summary>
        public bool ShowPopupWindow
        {
            get { return _showPopupWindow; }
            set 
            {
                if (value)
                {
                }
                _showPopupWindow = value;
                OnPropertyChanged("ShowPopupWindow");
            }
        }

        /// <summary>
        /// gets or sets time when DispatcherTimer shold tick
        /// </summary>
        public DateTime EndTimeOfTick
        {
            get { return _endTime; }
            set 
            { _endTime = value; }
        }

        /// <summary>
        /// gets or sets seconds remaining until DispatcherTimer_tick event
        /// </summary>
        public int RemainingSeconds
        {
            get { return _remainingSeconds; }
            set 
            { 
                _remainingSeconds = value;
                OnPropertyChanged("RemainingSeconds");
            }
        }

        public string WorkItemTemplatesFile
        {
            set
            {
                WorkItemTemplateProvider.WorkItemTemplatesFile = value;
                CheckWorkItemSizeIsValid();
                OnPropertyChanged("StatusViewModel");
                OnPropertyChanged("AllowLargeWorkItems");
                OnPropertyChanged("AllowSmallWorkItems");
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

        #region Public Functions

        public string GetTargetState(WorkItem workItem, ICustomState targetState)
        {
            return TaskboardService.GetTargetState(workItem, targetState);
        }
        public void AutoConnect()
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => PerformAutoConnect();
            worker.RunWorkerCompleted += (sender, args) => worker.Dispose();
            worker.RunWorkerAsync();
        }

        public void QuickConnect()
        {
            var teamProjectPicker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            var result = teamProjectPicker.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            IsConnecting = true;
            
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => PerformQuickConnect(teamProjectPicker.SelectedTeamProjectCollection.Uri, teamProjectPicker.SelectedProjects[0].Name);
            worker.RunWorkerCompleted += (sender, args) => worker.Dispose();
            worker.RunWorkerAsync();
        }

        public void Connect (string fileName)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += (sender, args) => PerformConnect(fileName);
            worker.RunWorkerCompleted += (sender, args) => worker.Dispose();
            worker.RunWorkerAsync();
        }

        public void OpenConfiguration(string fileName)
        {
            IsConnecting = true;
            // inform configuration service to use file
            ConfigurationService.UseMemoryFile = false;
            // Note: When starting with a new config the config will be set up based on the current team project
            //       That's why we still need access to it
            var configuration = ConfigurationService.ReadTaskboardConfiguration(fileName);

            // A configuration always contains it's connection data. Therefore we disconnect the taskboard service
            // and reconnect it afterwards using the connection data from the configuration
            TaskboardService.Disconnect();

            if (!configuration.IsAssociatedWithTeamProject)
            {
                EnsureConfigurationModel();
                ConfigurationViewModel.ConfigurationData = ConfigurationFileName = fileName;
                IsConnecting = false;
                return;
            }

            bool connectSucceeded = false;
            EventHandler<LoginDataEventArgs> handleRequestLoginData = delegate(object sender, LoginDataEventArgs args)
                                                                          {
                                                                              args.LoginData.TeamProjectCollectionUri = new Uri(configuration.TeamProjectCollection);
                                                                              args.LoginData.TeamProjectName = configuration.TeamProject;
                                                                          };
            EventHandler handleTaskboardServiceConnected = delegate { connectSucceeded = true; };
            
            TaskboardService.RequestLoginData += handleRequestLoginData;
            TaskboardService.Connected += handleTaskboardServiceConnected;
            TaskboardService.Connect();
            TaskboardService.RequestLoginData -= handleRequestLoginData;
            TaskboardService.Connected -= handleTaskboardServiceConnected;

            if (connectSucceeded)
            {
                ConfigurationFileName = fileName;
                MruFileManager.AddFile(fileName);
                EnsureConfigurationModel();    
            }
            else
            {
                // TODO: Handle the case when connection was not possible
                ConfigurationFileName = null;
                EnsureConfigurationModel();
            }
            IsConnecting = false;
        }

        public void SaveChanges()
        {
            TaskboardService.WorkItemIssuesOccurred += HandlWorkItemIssuesOccurred;
            TaskboardService.SaveChanges();
            TaskboardService.WorkItemIssuesOccurred -= HandlWorkItemIssuesOccurred;

            RefreshFilter();
            
        }       

        public void RefreshFilter()
        {
            _assignedToFilterList = null;
            _iterationPaths = null;
            _areaPaths = null;
            OnPropertyChanged("AssignedToFilterList");
            OnPropertyChanged("AssignedToFilterSelectedItem");
            OnPropertyChanged("IterationPaths");
            OnPropertyChanged("AreaPaths");
            OnSelectFilterItem();
        }

        public void RefreshSummaries()
        {
            ConfigurationViewModel.RefreshSummaries();
        }

        public void Print (WorkItem workItem)
        {
            // print WorkItem
            PrintWorkItems(new List<WorkItem> { workItem });
        }

        public void Print (IBacklogChildren backLogChildren)
        {
            // print Backlog WI and all its child WIs
            List<WorkItem> workItemList = new List<WorkItem>();
            
            // insert backlog WI into list
            workItemList.Add(backLogChildren.Backlog);

            // insert backlog child WIs which are visible in grid
            foreach (var state in backLogChildren.States)
            {
                var workItems = backLogChildren.Children.Where(c => state.WorkItemStates.Contains(c.State));
                foreach (var item in workItems)
                    workItemList.Add(item);
            }

            // print all WorkItems in list
            PrintWorkItems(workItemList);
        }

        public void Print (IList<WorkItem> workItems)
        {
            // print all WorkItems
            PrintWorkItems(workItems);
        }

        private void HandlWorkItemIssuesOccurred(object sender, WorkItemIssueEventArgs e)
        {
            if (e.WorkItemIssues.Count > 0)
                OnWorkItemIssues(e);
        }
        private void OnWorkItemIssues(WorkItemIssueEventArgs e)
        {
            if (WorkItemIssues != null)
                WorkItemIssues(this, e);
        }
        public event EventHandler<WorkItemIssueEventArgs> WorkItemIssues;

        public WorkItem CreateNewLinkedWorkItem()
        {
            var item = TaskboardService.CreateNewLinkedWorkItem(SelectedWorkItem,
                                                     LinkedWorkItemViewModel.SelectedLinkTypeEnd,
                                                     LinkedWorkItemViewModel.SelectedWorkItemType,
                                                     LinkedWorkItemViewModel.NewWorkItemTitle,
                                                     LinkedWorkItemViewModel.NewWorkItemComment);

            return item;
        }

        public void SaveLinkedItem(WorkItem item)
        {
            TaskboardService.SaveLinkedItem(SelectedWorkItem, item, LinkedWorkItemViewModel.SelectedLinkTypeEnd);
        }

        public bool IsVisibleNewLinkedWorkItem(WorkItem item, WorkItemLinkTypeEnd selectedLinkTypeEnd)
        {
            bool isStateVisible = false;
            foreach (ICustomState customState in ConfigurationViewModel.CustomStates)
            {
                if (customState.WorkItemStates.Contains(item.State))
                {
                    isStateVisible = true;
                    break;
                }
            }
            bool isLinkTypeEndVisible = ConfigurationViewModel.CurrentLinkType == null ||
                ConfigurationViewModel.CurrentLinkType.ForwardEnd == selectedLinkTypeEnd ||
                ConfigurationViewModel.CurrentLinkType.ReverseEnd == selectedLinkTypeEnd;
            if (isLinkTypeEndVisible && isStateVisible && ConfigurationViewModel.ChildItems.Contains(item.Type.Name))
            {
                TaskboardService.ApplyConfigurationToLink(item, SelectedWorkItem);
                return true;
            }
            return false;
        }

        #endregion

        #region Private Functions
        private void HandleRequestLoginData(object sender, LoginDataEventArgs args)
        {
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(new Action<object, LoginDataEventArgs>(HandleRequestLoginData), sender, args);
                return;
            }

            var picker = new TeamProjectPicker(TeamProjectPickerMode.SingleProject, false);
            // TODO: Set Team Project Collection and Team Project according to the login data
            WinForms.DialogResult result = picker.ShowDialog();
            if (result == WinForms.DialogResult.OK)
            {
                args.LoginData.TeamProjectCollectionUri = picker.SelectedTeamProjectCollection.Uri;
                args.LoginData.TeamProjectName = picker.SelectedProjects[0].Name;
            }
            else
            {
                args.Cancel = true;
            }
        }

        private void PerformAutoConnect()
        {
            if (MruFileManager.MostRecentlyUsedFiles.Count > 0)
            {
                OpenConfiguration(MruFileManager.MostRecentlyUsedFiles[0]);
            }
        }

        private void PerformQuickConnect(Uri teamProjectCollectionUri, string projectName)
        {
            // Before connecting to a new team project we close the current configuration
            ConfigurationFileName = null;
            ConfigurationService.UseMemoryFile = true;
            
            TaskboardService.Disconnect();
            ConfigurationViewModel.QueryItem = null;
            OnPropertyChanged("IsConnected");

            // Now prepare for connection and connect. In case the service cannot connect it will ask for login data...and we will answer
            TaskboardService.LoginData = Factory.CreateObject<ILoginData>();
            TaskboardService.LoginData.TeamProjectCollectionUri = teamProjectCollectionUri;
            TaskboardService.LoginData.TeamProjectName = projectName;

            TaskboardService.Connect();
            IsConnecting = false;
            OnConnectCompleted();
            OnPropertyChanged("IsConnected");
        }

        private void PerformConnect(string fileName)
        {
            var config = ConfigurationService.ReadTaskboardConfiguration(fileName);
            ILoginData loginData = Factory.CreateObject<ILoginData>();
            loginData.TeamProjectCollectionUri = new Uri(config.TeamProjectCollection);
            loginData.TeamProjectName = config.TeamProject;
            TaskboardService.LoginData = loginData;
            IsConnecting = true;
            TaskboardService.Connect();
            OnConnectCompleted();
            OnPropertyChanged("IsConnected");
            OpenConfiguration(fileName);
            IsConnecting = false;
        }

        private void HandleAutoConnectFailure(object sender, ErrorEventArgs e)
        {
            // Remove event handlers registered for Auto connect
            TaskboardService.Connected -= HandleAutoConnectSucess;
            TaskboardService.ConnectionFailed -= HandleAutoConnectFailure;
            TaskboardService.TfsCommunicationError -= HandleAutoConnectFailure;
            OnConnectCompleted();
            IsConnecting = false;
        }

        private void HandleAutoConnectSucess(object sender, EventArgs e)
        {
            // Remove event handlers registered for auto connect
            TaskboardService.Connected -= HandleAutoConnectSucess;
            TaskboardService.ConnectionFailed -= HandleAutoConnectFailure;
            TaskboardService.TfsCommunicationError -= HandleAutoConnectFailure;

            // Connection succeeded. Load the configuration
            if (MruFileManager.MostRecentlyUsedFiles.Count > 0)
            {
                string lastFile = MruFileManager.MostRecentlyUsedFiles[0];
                if (File.Exists(lastFile))
                    OpenConfiguration(lastFile);
            }
            OnConnectCompleted();
            IsConnecting = false;
        }

        private void HandleTfsCommunicationError(object sender, ErrorEventArgs e)
        {
            if (StatusService != null)
            {
                StatusService.EnqueueStatusItem("TfsCommunicationError");
            }
        }

        private void HandleConnectionFailed(object sender, ErrorEventArgs e)
        {
            // disconnect on connect error
            ConfigurationFileName = null;
            TaskboardService.Disconnect();
            EnsureConfigurationModel();
            OnPropertyChanged("IsConnected");
            SelectedWorkItem = null;
            OnConnectCompleted();
            IsConnecting = false;
            // TODO: Visualize the issue
        }

        private void HandleConfigurationApplied(object sender, EventArgs e)
        {
            OnPropertyChanged("ConfigurationViewModel");
            OnPropertyChanged("StatusViewModel");
            OnPropertyChanged("ReportViewModel");
            OnPropertyChanged("IsConnected");
            OnPropertyChanged("IsConnecting");
        }

        public void EnsureConfigurationModel()
        {
            if (string.IsNullOrEmpty(ConfigurationFileName))
            {
                // Reset all Models
                StatusViewModel = new StatusViewModel(TaskboardService, StatusService, null, WorkItemTemplateProvider,
                                                      Logger);
                ReportViewModel = new ReportViewModel(TaskboardService, ConfigurationService, null)
                                      {HideReports = false};
                ConfigurationViewModel = new ConfigurationViewModel(TaskboardService, ConfigurationService,
                                                                    StatusService, Factory, Logger);
                LinkedWorkItemViewModel = new LinkedWorkItemViewModel(TaskboardService, Logger);
            }
            else
            {
                // Ensure to setup the configuration view model
                ConfigurationViewModel = new ConfigurationViewModel(TaskboardService, ConfigurationService,
                                                                    StatusService, Factory, Logger)
                                             {ConfigurationData = ConfigurationFileName};
                LinkedWorkItemViewModel = new LinkedWorkItemViewModel(TaskboardService, Logger);
                //listen for PropertyChanged event so that we know when CustomStates was updated
                ConfigurationViewModel.PropertyChanged += ConfigurationViewModel_PropertyChanged;
                try
                {
                    ConfigurationViewModel.LoadConfiguration();

                    if (ShowReportViewBarChangedFromUI)
                    {
                        ConfigurationViewModel.HideReportViewer = ShowReportViewerBar;
                        ShowReportViewBarChangedFromUI = false;
                    }
                    else
                    {
                        if (!CreateNewConfigurationFromExisting)
                        {
                            ShowReportViewerBar = ConfigurationViewModel.HideReportViewer;
                        }
                        else
                        {
                            CreateNewConfigurationFromExisting = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    StatusService.DequeueStatusItem("ApplyConfiguration");
                    var errorMessage = StatusService.EnqueueStatusItem("ConfigurationError");
                    errorMessage.Message = "Configuration could not be loaded";
                    return;
                }
                ConfigurationViewModel.PropertyChanged -= ConfigurationViewModel_PropertyChanged;

                // Build a report view model that fits to the current configuration
                ReportViewModel = new ReportViewModel(TaskboardService, ConfigurationService,
                                                      ConfigurationViewModel.Configuration)
                                      {
                                          ConfigurationData = ConfigurationFileName,
                                          HideReports = ConfigurationViewModel.HideReportViewer
                                      };
                // Build a status view model that fits to the current configuration
                StatusViewModel = new StatusViewModel(TaskboardService, StatusService,
                                                      ConfigurationViewModel.Configuration,
                                                      WorkItemTemplateProvider, Logger);
                RefreshFilter();
                //check if configuration size is valid. otherwise revert to default
            }
            OnPropertyChanged("ConfigurationViewModel");
            OnPropertyChanged("StatusViewModel");
            OnPropertyChanged("ReportViewModel");

            StatusService.DequeueStatusItem("ApplyConfiguration");
            SyncService.BeginInvoke(DispatcherPriority.Background, new Action<bool>(ConfigurationViewModel.Commit),
                                    false);

            OnSelectFilterItem();
        }

        public void CheckWorkItemSizeIsValid()
        {
            if (!WorkItemTemplateProvider.IsSizeDefined(ConfigurationViewModel.WorkItemSize))
            {
                SelectedWorkItemSize = Interface.WorkItemSize.Medium;
            }
        }

        void ConfigurationViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CustomStates")
                OnPropertyChanged("CustomStates");

            if (e.PropertyName == "WorkItemSize")
            {
                OnPropertyChanged("WorkItemSize");
                OnPropertyChanged("WorkItemWidth");
                OnPropertyChanged("WorkItemHeight");
                OnPropertyChanged("SelectedWorkItemSize");
            }
        }

        private void PrintWorkItems(IList<WorkItem> workItems)
        {
            var printDialog = new PrintDialog();
            if ((bool)printDialog.ShowDialog().GetValueOrDefault())
            {
                var paginator = new WorkItemDocumentPaginator(this, workItems);
                paginator.PageSize = new Size(printDialog.PrintableAreaWidth,
                                              printDialog.PrintableAreaHeight);
                printDialog.PrintDocument(paginator, Application.Current.MainWindow.Title);
            }
        }

        
        #endregion

        public event EventHandler ConnectCompleted;
        private void OnConnectCompleted()
        {
            if (ConnectCompleted != null)
                ConnectCompleted(this, EventArgs.Empty);
        }

        public event EventHandler SelectFilterItem;
        private void OnSelectFilterItem()
        {
            if (SelectFilterItem != null)
                SelectFilterItem(this, EventArgs.Empty);
        }
    }
    
}
