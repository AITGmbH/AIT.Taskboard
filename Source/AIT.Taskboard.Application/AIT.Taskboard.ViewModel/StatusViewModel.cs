using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using AIT.Taskboard.Interface;
using System.ComponentModel.Composition;

namespace AIT.Taskboard.ViewModel
{
    public class StatusViewModel : INotifyPropertyChanged
    {
        public StatusViewModel(ITaskboardService taskboardService, IStatusService statusService, IConfiguration configuration, IWorkItemTemplateProvider workItemTemplateProvider, ILogger logger)
        {
            TaskboardService = taskboardService;
            TaskboardService.ConnectionFailed += HandleConnectionFailed;
            TaskboardService.TfsCommunicationError += HandleCommunicationError;
            TaskboardService.Connected += HandleConnected;
            TaskboardService.Disconnected += HandleDisconnected;
            StatusService = statusService;
            StatusService.PropertyChanged += (sender, args) => OnPropertyChanged(string.Empty);
            Configuration = configuration;
            WorkItemTemplateProvider = workItemTemplateProvider;
            Logger = logger;
        }

        private ILogger Logger { get; set; }
        private IConfiguration Configuration { get; set; }
        private IStatusService StatusService { get; set; }
        private ITaskboardService TaskboardService { get; set; }
        private IWorkItemTemplateProvider WorkItemTemplateProvider { get; set; }

        private void HandleDisconnected(object sender, EventArgs e)
        {
            OnPropertyChanged(string.Empty);
        }
        private void HandleConnected(object sender, EventArgs e)
        {
            OnPropertyChanged(string.Empty);
        }
        private void HandleCommunicationError(object sender, ErrorEventArgs e)
        {
            var item = StatusService.EnqueueStatusItem("CommunicationError");
            item.Message = string.Format("A communication error occured: {0}", e.ErrorMessage);
            OnPropertyChanged(string.Empty);
        }
        private void HandleConnectionFailed(object sender, ErrorEventArgs e)
        {
            var item = StatusService.EnqueueStatusItem("ConnectionError");
            item.Message = string.Format("A connection error occured: {0}", e.ErrorMessage);
            OnPropertyChanged(string.Empty);
        }

        public bool IsProgressing { get { return StatusService.IsProgressing; } }
        public bool HasTeamProjectCollection { get { return TaskboardService.IsConnected; } }
        public bool HasTeamProject { get { return TaskboardService.IsConnected; } }
        public bool HasWorkItemQuery { get { return TaskboardService.IsConnected; } }
        public bool HasRowSummary { get { return Configuration != null && !String.IsNullOrEmpty(Configuration.RowSummaryFieldName); } }
        public bool ShowColumnSummary { get { return Configuration != null && Configuration.HideColumnSummaryFieldName && !String.IsNullOrEmpty(Configuration.ColumnSummaryFieldName); } }
        public bool HasStyleFile { get { return WorkItemTemplateProvider != null && !String.IsNullOrEmpty(WorkItemTemplateProvider.WorkItemTemplatesFile); } }

        public string WorkItemQuery
        {
            get
            {
                if ((TaskboardService.IsConnected)&&(Configuration != null) &&(Configuration.QueryId != Guid.Empty))
                {
                    try
                    {
                        return TaskboardService.GetQueryItem(Configuration.QueryId).Name;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogException(ex);
                    }
                }
                return null;
            }
        }

        public string TeamProject
        {
            get
            {
                if (TaskboardService.LoginData != null)
                    return TaskboardService.LoginData.TeamProjectName;
                return null;
            }
        }

        public string TeamProjectCollection
        {
            get
            {
                if ((TaskboardService.LoginData != null) && (TaskboardService.LoginData.TeamProjectCollectionUri != null))
                    return TaskboardService.LoginData.TeamProjectCollectionUri.ToString();
                return null;
            }
        }

        public string CurrentStatusText
        {
            get
            {
                return StatusService.CurrentStatusText;
            }
        }

        public int ProgressPercentComplete { get { return StatusService.ProgressPercentComplete; } }
        public bool IsProgressIndeterminate { get { return StatusService.IsProgressIndeterminate; } }

        public string RowSummary
        {
            get
            {
                if (HasRowSummary)
                    return "Row summary: " + Configuration.RowSummaryFieldName;
                return null;
            }
        }

        public string ColumnSummary
        {
            get
            {
                if (ShowColumnSummary)
                    return "Column summary: " + Configuration.ColumnSummaryFieldName;
                return null;
            }
        }

        public string StyleFileName
        {
            get
            {
                if (HasStyleFile)
                    return "Style: " + System.IO.Path.GetFileNameWithoutExtension(WorkItemTemplateProvider.WorkItemTemplatesFile);
                return null;
            }
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged (string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
