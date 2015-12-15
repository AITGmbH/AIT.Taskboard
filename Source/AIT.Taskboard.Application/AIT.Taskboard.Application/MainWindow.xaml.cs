using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using AIT.Taskboard.Application.Controls;
using AIT.Taskboard.Application.EmailCreation;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Application.Properties;
using AIT.Taskboard.Application.UIInteraction;
using AIT.Taskboard.Interface;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.Win32;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Controls;
using System.Windows.Media;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [Export(typeof(ISyncService))]
    [Export("MainWindow", typeof(Window))]
    public partial class MainWindow : ISyncService
    {
        private const int SecondsToShowPopupWindow = 30;

        private ApplicationViewModel _viewModel;
        private WindowState _windowStateBeforeFullScreen;
        private delegate void InvokeDelegate();
        private DispatcherTimer _dispatcherTimer;
        private DispatcherTimer _helpDispatcherTimer;
        private bool _canRefreshNow;
        
        public MainWindow()
        {
            InitializeComponent();
            InitializeDispatcherTimer();
            CanRefreshNow = true;
        }

        [Import]
        private ApplicationViewModel ViewModel
        {
            get { return _viewModel; }
            set
            {
                // The view model is accessed from different threads. If we would just return the DataContext without keeping a separate field
                // we would need to switch to the UI thread every thime we access the ViewModel. Therfore a sample backing field was introduced.                
                    _viewModel = value;
                DataContext = value;
            }
        }

        [Import]
        private CompositionContainer Container { get; set; }

        [Import]
        private ILogger Logger { get; set; }

        [Import]
        private IEmailCreation EmailCreation { get; set; }

        /// <summary>
        /// Timer used with AutoRefresh
        /// </summary>
        private DispatcherTimer DispatcherTimer
        {
            get { return _dispatcherTimer; }
            set { _dispatcherTimer = value; }
        }

        /// <summary>
        /// Timer used to decide when to show popup notification about upcoming AutoRefresh
        /// </summary>
        private DispatcherTimer HelpDispatcherTimer
        {
            get { return _helpDispatcherTimer; }
            set { _helpDispatcherTimer = value; }
        }

        /// <summary>
        /// Tells whether refresh can be started now
        /// </summary>
        private bool CanRefreshNow
        {
            get { return _canRefreshNow; }
            set { _canRefreshNow = value; }
        }

        #region Private Methods

        private void CheckChanges()
        {
            if (ViewModel.WorkItemsHaveChanges)
            {
                if (MessageBox.Show(Properties.Resources.SaveChanges, Properties.Resources.SaveChangesHeader, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes) == MessageBoxResult.Yes)
                {
                    ViewModel.WorkItemIssues += HandleWorkItemIssues;
                    ViewModel.SaveChanges();
                    ViewModel.WorkItemIssues -= HandleWorkItemIssues;
                }
            }
        }

        private void HandleWorkItemIssues(object sender, WorkItemIssueEventArgs e)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<object, WorkItemIssueEventArgs>(HandleWorkItemIssues), sender, e);
                return;
            }
            var model = new WorkItemIssueViewModel(e.WorkItemIssues);
            var issueWindow = new WorkItemIssueWindow
                                  {
                                      Model = model,
                                      ApplicationViewModel = ViewModel,
                                      TaskboardControl = taskboardContent.taskboardControl,
                                      Owner = this,
                                      DataContext = model
                                  };

            issueWindow.ShowDialog();
        }

        /// <summary>
        /// Refreshes the taskboard
        /// </summary>
        private void DoRefreshTaskboard()
        {
            CanRefreshNow = false;
            CheckChanges();
            if ((ViewModel != null) && (ViewModel.ConfigurationViewModel != null))
            {
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(RefreshDoWork);
                bw.RunWorkerAsync();
            }
            CanRefreshNow = true;
        }

        /// <summary>
        /// Initializes timer used with AutoRefresh
        /// </summary>
        private void InitializeDispatcherTimer()
        {
            DispatcherTimer = new DispatcherTimer();
            HelpDispatcherTimer = new DispatcherTimer();
            DispatcherTimer.Tick += new EventHandler(DispatcherTimer_tick);
            HelpDispatcherTimer.Tick += new EventHandler(HelpDispatcherTimer_tick);
            DispatcherTimer.Interval = new TimeSpan(0, 10, 0);
            HelpDispatcherTimer.Interval = new TimeSpan(0, 0, 1);
        }

        /// <summary>
        /// Tick event of timer used with AutoRefresh
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event data</param>
        private void DispatcherTimer_tick(object sender, EventArgs e)
        {
            StopDispatcherTimer();
            if (!ViewModel.WorkItemsHaveChanges && ((ViewModel.SelectedWorkItem != null && !ViewModel.SelectedWorkItem.IsOpen) || ViewModel.SelectedWorkItem == null))
            {
                if (AutoRefreshToggleBox.IsChecked == true)
                {
                    DoRefreshTaskboard();
                }
                else
                {
                    StartDispatcherTimer();
                }
            }
            else
            {
                StartDispatcherTimer();
            }
            ViewModel.ShowPopupWindow = false;    
        }

        /// <summary>
        /// tick event of help timer used to decide when to show popup window
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event data</param>
        private void HelpDispatcherTimer_tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            TimeSpan span = ViewModel.EndTimeOfTick - now;
            if (!ViewModel.WorkItemsHaveChanges && ((ViewModel.SelectedWorkItem != null && !ViewModel.SelectedWorkItem.IsOpen) || ViewModel.SelectedWorkItem == null))
            {
                if (span.TotalSeconds < SecondsToShowPopupWindow)
                {
                    ViewModel.RemainingSeconds = span.Seconds;
                    ViewModel.ShowPopupWindow = true;
                }
                else
                {
                    ViewModel.ShowPopupWindow = false;
                }
            }
            else
            {
                StopDispatcherTimer();
                StartDispatcherTimer();
            }
        }

        /// <summary>
        /// Starts timer used with AutoRefresh
        /// </summary>
        private void StartDispatcherTimer()
        {
            if (ViewModel != null && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.ConfigurationData != null)
            {
                IConfiguration config = ViewModel.ConfigurationService.ReadTaskboardConfiguration(ViewModel.ConfigurationViewModel.ConfigurationData);
                DispatcherTimer.Interval = new TimeSpan(0, config.AutoRefreshDelay, 0);
                ViewModel.EndTimeOfTick = DateTime.Now + DispatcherTimer.Interval;
                DispatcherTimer.Start();
                HelpDispatcherTimer.Start();
                ResetFilter();
            }
        }

        /// <summary>
        /// Stops timer used with AutoRefresh
        /// </summary>
        private void StopDispatcherTimer()
        {
            DispatcherTimer.Stop();
            HelpDispatcherTimer.Stop();
            if (ViewModel.ShowPopupWindow)
            {
                ViewModel.ShowPopupWindow = false;
            }
        }

        /// <summary>
        /// Called when .tbconfig loaded to set AutoRefreshToggleBox.IsChecked property
        /// </summary>
        private void LoadAutoRefreshStatus()
        {
            if (ViewModel != null && ViewModel.IsConnected && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.IsConfigurationLoaded)
            {
                if (Dispatcher.CheckAccess())
                {
                    IConfiguration config = ViewModel.ConfigurationService.ReadTaskboardConfiguration(ViewModel.ConfigurationViewModel.ConfigurationData);
                    AutoRefreshToggleBox.IsChecked = config.IsAutoRefreshChecked;
                    if (AutoRefreshToggleBox.IsChecked == true)
                    {
                        StartDispatcherTimer();
                    }                    
                }
                else
                {
                    Dispatcher.Invoke(new Action(LoadAutoRefreshStatus));
                }
            }
        }

        #endregion Private Methods

        #region Connect and Exit

        private void ExecuteQuickConnect(object sender, ExecutedRoutedEventArgs e)
        {
            CheckChanges();
            ViewModel.ConnectCompleted += HandleQuickConnectCompleted;
            ViewModel.QuickConnect();
        }

        private void CanExecuteQuickConnect(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && !ViewModel.IsConnecting;
        }
       
        private void ExecuteExitApplication(object sender, ExecutedRoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        #endregion

        #region Configuration Handling
        private void ExecuteSaveConfiguration(object sender, ExecutedRoutedEventArgs e)
        {
            PerformSaveConfiguration();
            LoadAutoRefreshStatus();
        }

        private void PerformSaveConfiguration()
        {
            var dialog = new SaveFileDialog
            {
                Filter = Properties.Resources.TaskboardConfigFileFilter,
                Title = Properties.Resources.SaveFileDialogTitle,
                DefaultExt = Properties.Resources.DefaultConfigFileExtension,
                OverwritePrompt = true
            };
            bool? result = dialog.ShowDialog(this);
            if (result.HasValue && result.Value)
            {
                string fileName = dialog.FileName;
                ViewModel.ConfigurationViewModel.ConfigurationData = fileName;
                ViewModel.ConfigurationFileName = fileName;
                ViewModel.ConfigurationViewModel.Commit(true);
                //update the file name so the window title does not show [Read-Only] any more
                ViewModel.ConfigurationFileName = fileName;
            }
        }

        private void CanExecuteSaveConfiguration(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationService.UseMemoryFile;
        }

        private void ExecuteNewConfiguration(object sender, ExecutedRoutedEventArgs e)
        {
            StopDispatcherTimer();
            CheckChanges();

            var dialog = new SaveFileDialog
            {
                Filter = Properties.Resources.TaskboardConfigFileFilter,
                Title = Properties.Resources.SaveFileDialogTitle,
                DefaultExt = Properties.Resources.DefaultConfigFileExtension,
                OverwritePrompt = true
            };
            bool? result = dialog.ShowDialog(this);
            if (result.HasValue && result.Value)
            {
                ViewModel.CreateNewConfigurationFromExisting = true;
                // Create a new configuration
                ViewModel.OpenConfiguration(dialog.FileName);
                // Open the settings dialog for the new configuration
                ExecuteSettings(null, null);
            }
            LoadAutoRefreshStatus();
        }

        string configFilename;
        private void ExecuteOpenConfiguration(object sender, ExecutedRoutedEventArgs e)
        {
            StopDispatcherTimer();
            CheckChanges();

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = Properties.Resources.TaskboardConfigFileFilter;
            dialog.Title = Properties.Resources.OpenFileDialogTitle;
            dialog.DefaultExt = Properties.Resources.DefaultConfigFileExtension;
            dialog.Multiselect = false;
            bool? result = dialog.ShowDialog(this);
            if (result.HasValue && result.Value)
            {
                configFilename = dialog.FileName;
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += new DoWorkEventHandler(OpenConfigurationDoWork);
                bw.RunWorkerAsync();
            }
            LoadAutoRefreshStatus();
        }

        private void AutoConnect()
        {
            StopDispatcherTimer();
            if (string.IsNullOrEmpty(ApplicationHelper.FileToOpen))
                ViewModel.AutoConnect();
            else
            {
                if (File.Exists(ApplicationHelper.FileToOpen))
                {
                    ViewModel.Connect(ApplicationHelper.FileToOpen);
                }
                else
                {
                    ViewModel.AutoConnect();
                }
            }
        }

        void OpenConfigurationDoWork(object sender, DoWorkEventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                taskboardContent.ReBind(true);
            }
            else
            {
                ViewModel.OpenConfiguration(configFilename);
                // Invokation required
                Dispatcher.Invoke(new Action<object, DoWorkEventArgs>(OpenConfigurationDoWork), DispatcherPriority.Normal,
                                                      sender, e);
            }
        }

        void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "CustomStates" && e.PropertyName != "WorkItemSize")
                return;

            //recreate the grid when the columns have been changed
            if (Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                if(e.PropertyName == "CustomStates")
                    taskboardContent.ReBind(true);
                if (e.PropertyName == "WorkItemSize")
                    ViewModel.CheckWorkItemSizeIsValid();
            }
            else
            {
                // Invokation required
                Dispatcher.Invoke(new Action<object, PropertyChangedEventArgs>(ViewModel_PropertyChanged), DispatcherPriority.Normal,
                                                      sender, e);
            }
        }

        private void ExecuteSettings(object sender, ExecutedRoutedEventArgs e)
        {
            StopDispatcherTimer();
            CheckChanges();

            ViewModel.ConfigurationViewModel.HideReportViewer = ViewModel.ShowReportViewerBar;

            var configurationWindow = new ConfigurationWindow {Owner = this, Model = ViewModel.ConfigurationViewModel};

            if (ViewModel.IsConnected)
                try
                {
                    ViewModel.ConfigurationViewModel.RefreshQueryItems();
                }
                catch (Exception ex)
                {
                    Logger.LogException(ex);
                    return;
                }

            var result = configurationWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var bw = new BackgroundWorker();
                bw.DoWork += LoadSettingsDoWork;
                bw.RunWorkerAsync();
            }
            LoadAutoRefreshStatus();
        }

        void LoadSettingsDoWork(object sender, DoWorkEventArgs e)
        {
            //temporarily remove the handler so the columns are not reset on refresh
            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            //commit here, instead of the configuration window in order to prevent the Save->Load sequence
            ViewModel.ConfigurationViewModel.Commit(false);
            ViewModel.EnsureConfigurationModel();
            ViewModel.RefreshFilter();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            // Invokation required
            Dispatcher.Invoke(new Action<object, DoWorkEventArgs>(RefreshDoWork), DispatcherPriority.Normal,
                                                  sender, e);
        }

        private void CanExecuteNewConfiguration(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null;
        }

        private void CanExecuteSettings(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.IsConfigurationLoaded;
        }

        private void HandleMostRecentFileSelected(object sender, MouseButtonEventArgs e)
        {
            AppMenu.IsDropDownOpen = false;
            if (sender is TextBlock)
            {
                ViewModel.OpenConfiguration((sender as TextBlock).DataContext.ToString());
                taskboardContent.ReBind(true);
            }
        }
        #endregion
        
        #region Work Item Handling

        private void ExecuteRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            if (CanRefreshNow)
            {
                StopDispatcherTimer();
                DoRefreshTaskboard();
                StartDispatcherTimer();
            }
        }

        private void CanExecuteAddNewLinkedWorkItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && 
                ViewModel.SelectedWorkItem != null && 
                !ViewModel.TaskboardService.AllChildren.Contains(ViewModel.SelectedWorkItem);
               
        }

        private void ExecuteAddNewLinkedWorkItem(object sender, ExecutedRoutedEventArgs e)
        {
            taskboardContent.taskboardControl.AddLinkedWorkItem();
        }

        /// <summary>
        /// Executed method for command <see cref="AutoRefresh"/>.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event data</param>
        private void ExecuteAutoRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            if (AutoRefreshToggleBox.IsChecked.HasValue && ViewModel.ConfigurationViewModel.ConfigurationData != null)
            {
                IConfiguration config = ViewModel.ConfigurationService.ReadTaskboardConfiguration(ViewModel.ConfigurationViewModel.ConfigurationData);
                config.IsAutoRefreshChecked = (bool)AutoRefreshToggleBox.IsChecked;
                ViewModel.ConfigurationService.SaveTaskboardConfiguration(ViewModel.ConfigurationViewModel.ConfigurationData, config);
                if (AutoRefreshToggleBox.IsChecked == true)
                {
                    StopDispatcherTimer();
                    StartDispatcherTimer();
                }
                if (AutoRefreshToggleBox.IsChecked == false)
                {
                    StopDispatcherTimer();
                }
            }
        }

        void RefreshDoWork(object sender, DoWorkEventArgs e)
        {
            if (Dispatcher.CheckAccess())
            {
                // The calling thread owns the dispatcher, and hence the UI element
                taskboardContent.ReBind(false);
                ribbon.InvalidateVisual();
                StartDispatcherTimer();
            }
            else
            {
                //temporarily remove the handler so the columns are not reset on refresh
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.EnsureConfigurationModel();
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                // Invokation required
                Dispatcher.Invoke(new Action<object, DoWorkEventArgs>(RefreshDoWork), DispatcherPriority.Normal,
                                                      sender, e);
            }
        }

        private void ExecutePublish(object sender, ExecutedRoutedEventArgs e)
        {
            var saveWorker = new BackgroundWorker();
            saveWorker.DoWork += PerformPublish;
            saveWorker.RunWorkerCompleted += OnPerformPublishCompleted;
            saveWorker.RunWorkerAsync();
        }

        private void ExecutePrintAllItems(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.TaskboardService != null && ViewModel.TaskboardService.BacklogChildren.Count > 0)
            {
                // get all WIs which all visible in grid
                IList<WorkItem> items = new List<WorkItem>();
                foreach (var backlog in ViewModel.TaskboardService.BacklogChildren)
                {
                    items.Add(backlog.Backlog);
                    foreach(var state in backlog.States)
                    {
                        var workItems = backlog.Children.GetWorkItemsByState(state);
                        foreach (var workItem in workItems)
                            items.Add(workItem);
                    }
                }
                
                // print WIs
                ViewModel.Print(items);
            }
        }

        private void ExecutePrintBacklogItemAndChildren(object sender, ExecutedRoutedEventArgs e)
        {
            IBacklogChildren backlogChildren = ViewModel.TaskboardService.GetBacklogFromWorkItem(ViewModel.SelectedWorkItem);
            if (backlogChildren != null)
            {
                ViewModel.Print(backlogChildren);
            }
        }

        private void ExecutePrintCurrentItem(object sender, ExecutedRoutedEventArgs e)
        {
            if (ViewModel != null && ViewModel.SelectedWorkItem != null)
                ViewModel.Print(ViewModel.SelectedWorkItem);
        }

        private void CanExecuteRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.IsConfigurationLoaded;
        }

        /// <summary>
        /// CanExecute method for command <see cref="AutoRefresh"/>.
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="e">event data</param>
        private void CanExecuteAutoRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.IsConfigurationLoaded;
        }

        private void CanExecutePublish(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.ConfigurationViewModel != null && ViewModel.ConfigurationViewModel.IsConfigurationLoaded && ViewModel.WorkItemsHaveChanges;
        }

        private void CanExecutePrintAllItems(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.TaskboardService != null && ViewModel.TaskboardService.BacklogChildren.Count > 0;
        }

        private void CanExecutePrintBacklogItemAndChildren(object sender, CanExecuteRoutedEventArgs e)
        {

            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.SelectedWorkItem != null;
        }

        private void CanExecutePrintCurrentItem(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel != null && ViewModel.IsConnected && ViewModel.SelectedWorkItem != null;
        }

        private void CanExecuteShowHideReportViewerbar(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel == null || ViewModel.TaskboardService == null)
            {
                e.CanExecute = false;
            }
            else
            {
                if (ViewModel.TaskboardService.ReportServerUrl == null)
                {
                    e.CanExecute = true;
                }
                else
                {
                    e.CanExecute = !ViewModel.TaskboardService.ReportServerUrl.Equals(string.Empty);
                }
            }
        }

        private void OnPerformPublishCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            if (worker != null)
            {
                worker.DoWork -= PerformPublish;
                worker.RunWorkerCompleted -= OnPerformPublishCompleted;
            }
        }

        private void PerformPublish(object sender, DoWorkEventArgs e)
        {
            ViewModel.WorkItemIssues += HandleWorkItemIssues;
            ViewModel.SaveChanges();
            ViewModel.WorkItemIssues -= HandleWorkItemIssues;
            InvokeDelegate del = () =>
            {
                taskboardContent.ReBind(false);
                ReSelectAssignedToFilterItem();
                ReSelectChildrenFilterItem();
            };

            Dispatcher.Invoke(del, DispatcherPriority.Background);
        }
        #endregion

        #region Filter
        private void Filter_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            InvokeDelegate del = () =>
            {
                if (e.NewValue != null)
                {
                    var item = e.NewValue as RibbonGalleryItem;
                    if (sender == Filter)
                        ViewModel.AssignedToFilterSelectedItem = (string)item.Content;
                    else
                        ViewModel.ChildrenFilterSelectedItem = (string)item.Name;
                    var grid = WpfHelper.FindVisualChild<DataGrid>(this);
                    if (grid != null)
                    {
                        var workItemControl = WorkItemControl.SelectedItem;
                        if (workItemControl != null)
                        {
                            workItemControl.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                        }
                    }
                }
                else ResetFilter();

            };
            Dispatcher.Invoke(del, DispatcherPriority.DataBind);           
        }

        private void AreaFilter_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            InvokeDelegate del = () =>
            {
                if (e.NewValue != null)
                {
                    ViewModel.SelectedAreaPath = (string)e.NewValue;
                }
            };
            Dispatcher.Invoke(del, DispatcherPriority.DataBind);
        }

        private void IterationFilter_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            InvokeDelegate del = () =>
            {
                if (e.NewValue != null)
                {
                    ViewModel.SelectedIterationPath = (string)e.NewValue;
                }
            };
            Dispatcher.Invoke(del, DispatcherPriority.DataBind);
        }               
       
        private void ReSelectChildrenFilterItem()
        {
            foreach (RibbonGalleryItem item in (FilterByChildren.Items[0] as RibbonGalleryCategory).Items)
            {
                item.IsSelected = ((string)item.Name == ViewModel.ChildrenFilterSelectedItem);
            }
        }

        private void ReSelectAssignedToFilterItem()
        {
            foreach (RibbonGalleryItem item in (Filter.Items[0] as RibbonGalleryCategory).Items)
            {
                item.IsSelected = ((string)item.Content == ViewModel.AssignedToFilterSelectedItem);
            }
        }
        #endregion

        #region View Handling

        private void ExecuteFullScreen(object sender, ExecutedRoutedEventArgs e)
        {
            ribbon.Visibility = Visibility.Collapsed;
            status.Visibility = Visibility.Collapsed;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.Topmost = true;
            this.ResizeMode = System.Windows.ResizeMode.NoResize;
            _windowStateBeforeFullScreen = this.WindowState;
            this.WindowState = System.Windows.WindowState.Maximized;
        }

        private void CanExecuteFullScreen(object sender, CanExecuteRoutedEventArgs e)
        {
            if(null != ViewModel)
                e.CanExecute = ViewModel.IsConnected;
        }

        private void ExitFullScreen()
        {
            ribbon.Visibility = Visibility.Visible;
            status.Visibility = Visibility.Visible;
            this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            this.Topmost = false;
            this.ResizeMode = System.Windows.ResizeMode.CanResize;
            this.WindowState = _windowStateBeforeFullScreen;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ExitFullScreen();
            }
        }

        private void ExecuteZoomIn(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ZoomFactor += ApplicationViewModel.ZoomStep;
        }

        private void CanExecuteZoomIn(object sender, CanExecuteRoutedEventArgs e)
        {
            if (null != ViewModel)
                e.CanExecute = ViewModel.IsConnected && (ViewModel.ZoomFactor < ApplicationViewModel.MaxZoomFactor || ViewModel.AllowLargerSize);
        }

        private void ExecuteZoomOut(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ZoomFactor -= ApplicationViewModel.ZoomStep;
        }

        private void CanExecuteZoomOut(object sender, CanExecuteRoutedEventArgs e)
        {
            if (null != ViewModel)
                e.CanExecute = ViewModel.IsConnected && (ViewModel.ZoomFactor > ApplicationViewModel.MinZoomFactor || ViewModel.AllowSmallerSize);
        }

        private void ExecuteShowHideEditbar(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ShowEditbar = !ViewModel.ShowEditbar;
        }

        private void ExecuteShowHideReportViewerbar(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.ShowReportViewBarChangedFromUI = true;
            ViewModel.ShowReportViewerBar = !ViewModel.ShowReportViewerBar;
        }

        void WorkItemSizeChanged_Checked(object sender, RoutedEventArgs e)
        {
            // Only perform visual update of workitem zooming if no taskboard zoom ins in progress
            // to prevent the UI from causing visual glitches
            if (!TouchEventManager.IsZooming)
            {
                //the alternative would be to do a full refresh
                taskboardContent.taskboardControl.DataContext = null;
                taskboardContent.taskboardControl.DataContext = _viewModel;
            }
        }
        #endregion

        #region Help,  About and Feedback

        private void ExecuteHelp(object sender, ExecutedRoutedEventArgs e)
        {
            string helpFile = Path.Combine(ApplicationHelper.HelpTargetDirectory, "Taskboard.Quickstart.pdf");
            if (File.Exists(helpFile))
                Process.Start(new ProcessStartInfo(helpFile));
            else
            {
                MessageBox.Show(this, string.Format(Properties.Resources.HelpFileNotFoundMessage, helpFile),
                                Properties.Resources.HelpFileNotFoundTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteAbout(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new AboutWindow {Owner = this};
            Container.SatisfyImportsOnce(window);
            window.ShowDialog();
        }

        private void ExecuteSendFeedback(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo(ViewModel.FeedbackUri));
        }

        private void ExecuteSendFeedbackEmail(object sender, ExecutedRoutedEventArgs e)
        {
            var mailItem = new MailItem
                               {
                                   RecipientTo = (Settings.Default.RecipientTo),
                                   RecipientCC = (Settings.Default.RecipientCC),
                                   RecipientBCC = (Settings.Default.RecipientBCC),
                                   Subject = (Settings.Default.EmailSubject),
                                   Attachement = Logger.LogFile,
                                   Body = Logger.LogFile
                               };

            EmailCreation.CreateEmail(mailItem);
        }

        #endregion

        private void HandleMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            ViewModel.EnsureConfigurationModel();
            ViewModel.ConnectCompleted += HandleConnectCompleted;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.SelectFilterItem += HandleRefreshFilter;            
            ResetFilter();

            var galCategory = FilterByChildren.Items[0] as RibbonGalleryCategory;
            if (null != galCategory)
            {
                FilterByChildren.SelectedItem = galCategory.Items[1] as RibbonGalleryItem;
            }

            LoadWindowConfiguration();
            // TODO: Once the configuration stores it's connection data we need to use the file from command line and open it
            AutoConnect();            
        }       

        private void HandleConnectCompleted(object sender, EventArgs e)
        {
            // The access to the View Model as well as the rebind must occur on the UI thread.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<object, EventArgs>(HandleConnectCompleted), sender, e);
            }
            else
            {
                taskboardContent.ReBind(true);
                //force ribbon commands refresh
                ribbon.Focus();               
            }
            LoadAutoRefreshStatus();           
        }

        private void HandleRefreshFilter(object sender, EventArgs e)
        {
            // The access to the View Model as well as the rebind must occur on the UI thread.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<object, EventArgs>(HandleRefreshFilter), sender, e);
            }
            else
            {
                Filter.SelectedItem = null;
                ResetFilter();
                ribbon.Focus();
            }
        }

        private void ResetFilter()
        {
            var galCategory = Filter.Items[0] as RibbonGalleryCategory;
            if (null != galCategory && galCategory.Items.Count > 0)
            {
                if (null == ViewModel.AssignedToFilterSelectedItem)
                {
                    Filter.SelectedItem = galCategory.Items[0] as RibbonGalleryItem;
                }
                else 
                {
                    foreach (RibbonGalleryItem item in galCategory.Items)
                    {
                        if(item.Content.ToString().Equals(ViewModel.AssignedToFilterSelectedItem))
                            Filter.SelectedItem = item;
                    }
                }
            }
        }

        private void HandleQuickConnectCompleted(object sender, EventArgs e)
        {
            // The access to the View Model as well as the rebind must occur on the UI thread.
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(new Action<object, EventArgs>(HandleQuickConnectCompleted), sender, e);
            }
            else
            {
                ViewModel.ConnectCompleted -= HandleQuickConnectCompleted;
                if (ViewModel.IsConnected)
                {
                    var queryWindow = new QuerySelectWindow { Owner = this, Model = ViewModel.ConfigurationViewModel };
                    queryWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
                    if (ViewModel.IsConnected)
                        try
                        {
                            ViewModel.ConfigurationViewModel.RefreshQueryItems();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogException(ex);
                            return;
                        }

                    var result = queryWindow.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        ViewModel.IsConnecting = true;
                        BackgroundWorker worker = new BackgroundWorker();
                        worker.DoWork += (s, args) => LoadQuickConfigurationDoWork();
                        worker.RunWorkerCompleted += (s, args) =>
                        {
                            worker.Dispose(); 
                            LoadQuickConfigurationCompleted();
                        };
                        worker.RunWorkerAsync();
                    }
                    else
                    {
                        taskboardContent.Clear();
                    }
                }
            }
            LoadAutoRefreshStatus();
        }

        void LoadQuickConfigurationDoWork()
        {
            IConfiguration config = ViewModel.ConfigurationService.GetDefaultConfiguration();
            config.QueryId = ViewModel.ConfigurationViewModel.QueryItem.Id;
            ViewModel.ConfigurationFileName = "Temporary file";
            ViewModel.ConfigurationService.SaveTaskboardConfiguration(null, config);
            ViewModel.EnsureConfigurationModel();
        }

        void LoadQuickConfigurationCompleted()
        {
            ViewModel.IsConnecting = false;
            taskboardContent.ReBind(true);
            //force ribbon commands refresh
            ribbon.Focus();
        }

        #region Implementation of ISyncService

        public object Invoke(Delegate method, params object[] args)
        {
            return Dispatcher.Invoke(method, args);
        }

        public object Invoke(Delegate method, DispatcherPriority priority, params object[] args)
        {
            return Dispatcher.Invoke(method, priority, args);
        }

        public DispatcherOperation BeginInvoke(Delegate method, params object[] args)
        {
            return Dispatcher.BeginInvoke(method, args);
        }

        public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method, params object[] args)
        {
            if (args.Length == 1)
                return Dispatcher.BeginInvoke(priority, method, args[0]);
            return Dispatcher.BeginInvoke(priority, method, args);
        }

        public DispatcherOperation BeginInvoke(DispatcherPriority priority, Delegate method)
        {
            return Dispatcher.BeginInvoke(priority, method);
        }

        #endregion

        private void ExecuteSelectWorkItemResource(object sender, ExecutedRoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog();
            var currentTemplateFileName = ViewModel.WorkItemTemplateProvider.WorkItemTemplatesFile;
            if (File.Exists(currentTemplateFileName))
            {
                var fileInfo = new FileInfo(currentTemplateFileName);
                fileDialog.FileName = fileInfo.Name;
                fileDialog.InitialDirectory = fileInfo.DirectoryName;
                fileDialog.Filter = "Resource Dictionary (*.xaml)|*.xaml";
                fileDialog.Multiselect = false;
            }
            var dialogResult = fileDialog.ShowDialog(this);
            if (!dialogResult.GetValueOrDefault(false)) return;
            if (!File.Exists(fileDialog.FileName)) return;
            ViewModel.WorkItemTemplatesFile = fileDialog.FileName;
            taskboardContent.ReBind(true);
        }

        private void ExecuteResetWorkItemResource(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.WorkItemTemplatesFile = "";
            taskboardContent.ReBind(true);
        }

        #region Override methods
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (AutoRefreshToggleBox.IsEnabled && AutoRefreshToggleBox.IsChecked == true)
            {
                StopDispatcherTimer();
                StartDispatcherTimer();
            }
            base.OnPreviewMouseMove(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (AutoRefreshToggleBox.IsEnabled && AutoRefreshToggleBox.IsChecked == true)
            {
                StopDispatcherTimer();
                StartDispatcherTimer();
            }
            base.OnPreviewKeyDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (AutoRefreshToggleBox.IsEnabled && AutoRefreshToggleBox.IsChecked == true)
            {
                StopDispatcherTimer();
                StartDispatcherTimer();
            }
            base.OnPreviewMouseDown(e);
        }

        protected override void  OnClosing(CancelEventArgs e)
        {
            //persist settings from before entering full screen mode
            if (Topmost)
                ExitFullScreen();
            PersistWindowConfiguration();
            //we need to commit the configuration in order to store work item size, which is not modified from the configuration window
            ViewModel.ConfigurationViewModel.Commit(false);
            CheckChanges();
            if (ViewModel.ConfigurationService.UseMemoryFile)
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Would you like to save the current configuration?", "Save configuration", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
                if (messageBoxResult == MessageBoxResult.Yes)
                    PerformSaveConfiguration();
            }
 	        base.OnClosing(e);
        }
        #endregion Override methods

        #region Load and persist window configuration
        private void LoadWindowConfiguration()
        {
            if (Settings.Default.IsMaximized)
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Normal;
                Width = Settings.Default.Width;
                Height = Settings.Default.Height;
                Top = Settings.Default.Top;
                Left = Settings.Default.Left;
            }
            taskboardContent.SplitterPosition = Settings.Default.SplitterPosition;
        }

        private void PersistWindowConfiguration()
        {
            if (WindowState == System.Windows.WindowState.Maximized)
                Settings.Default.IsMaximized = true;
            else
            {
                Settings.Default.IsMaximized = false;
                Settings.Default.Width = Width;
                Settings.Default.Height = Height;
                Settings.Default.Top = Top;
                Settings.Default.Left = Left;
            }
            Settings.Default.SplitterPosition = taskboardContent.SplitterPosition;
            Settings.Default.Save();
        }
        #endregion        

    }
}
