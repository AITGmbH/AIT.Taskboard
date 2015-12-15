using System;
using System.Collections.Generic;
using System.ComponentModel;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.ViewModel
{
    public class LinkedWorkItemViewModel : INotifyPropertyChanged
    {
        private const string PreviewWorkItemDefault = "(New work item)";

        #region Constructors

        public LinkedWorkItemViewModel(ITaskboardService taskboardService, ILogger logger)
        {
            TaskBoardService = taskboardService;
            Logger = logger;
            LoadTypes();
            PreviewWorkItemTemplate = PreviewWorkItemDefault;
        }

        #endregion

        #region Properties

        private ILogger Logger { get; set; }

        public WorkItemTypeCollection WorkItemTypes { get; private set; }

        private WorkItemType _selectedWorkItemType;
        public WorkItemType SelectedWorkItemType
        {
            get
            {
                return _selectedWorkItemType;
            }
            set
            {
                _selectedWorkItemType = value;
                UpdatePreview();
                OnPropertyChanged("SelectedWorkItemType");
            }
        }

        private WorkItem _workItem;
        public WorkItem WorkItem 
        {
            get
            {
                return _workItem;
            }
            set 
            {
                _workItem = value;
                OnPropertyChanged("WorkItem");
            }
        }
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

        private WorkItemLinkTypeEnd _selectedLinkTypeEnd;
        public WorkItemLinkTypeEnd SelectedLinkTypeEnd
        {
            get
            {
                return _selectedLinkTypeEnd;
            }
            set 
            {
                _selectedLinkTypeEnd = value;
                OnPropertyChanged("SelectedLinkTypeEnd");
            }
        }

        private string _newWorkItemTitle;
        public string NewWorkItemTitle
        {
            get
            {
                return _newWorkItemTitle;
            }
            set
            {
                _newWorkItemTitle = value;
                UpdatePreview();
                OnPropertyChanged("NewWorkItemTitle");
            }
        }

        private string _newWorkItemComment;
        public string NewWorkItemComment
        {
            get
            {
                return _newWorkItemComment;
            }
            set
            {
                _newWorkItemComment = value;
                OnPropertyChanged("NewWorkItemComment");
            }
        }

        private string _previewWorkItemTemplate;
        public string PreviewWorkItemTemplate
        {
            get
            {
                return _previewWorkItemTemplate;
            }
            set
            {
                _previewWorkItemTemplate = value;
                OnPropertyChanged("PreviewWorkItemTemplate");
            }
        }

        private ITaskboardService TaskBoardService { get; set; }

        #endregion

        #region Methods

        public void ClearData()
        {
            NewWorkItemComment = string.Empty;
            NewWorkItemTitle = string.Empty;
        }

        private void LoadTypes()
        {
            WorkItemTypeCollection typesFromServer = TaskBoardService.GetWorkItemTypes();
            WorkItemTypes = typesFromServer;
            OnPropertyChanged("WorkItemTypes");

            LinkTypes = TaskBoardService.GetLinkTypes();
            OnPropertyChanged("LinkTypes");
        }

        private void UpdatePreview()
        {
            if (string.IsNullOrEmpty(NewWorkItemTitle))
            {
                PreviewWorkItemTemplate = PreviewWorkItemDefault;
            }
            else
            {
                PreviewWorkItemTemplate = "New " + SelectedWorkItemType.Name + ": " + NewWorkItemTitle;
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
