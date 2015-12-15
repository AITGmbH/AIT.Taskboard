using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.ViewModel
{
    public class WorkItemIssueViewModel: INotifyPropertyChanged
    {
        public WorkItemIssueViewModel(List<IWorkItemIssue> workItemIssues)
        {
            WorkItemIssues = new ObservableCollection<IWorkItemIssue>(workItemIssues);
        }

        public ObservableCollection<IWorkItemIssue> WorkItemIssues { get; private set; }
        public int PublishedItemsCount { get; private set; }
        public int TotalItemsCount 
        { 
            get;set;
        }

        public bool CanPublish
        {
            get
            {
                // We can publish the model if any of the work item issues is ready to publish
                return WorkItemIssues.Any(issue => issue.CanPublish);
            }
        }

        public WorkItem SelectedWorkItem 
        { 
            get
            {
                return SelectedWorkItemIssue != null ? SelectedWorkItemIssue.WorkItem : null;
            }
        }
        public IWorkItemIssue SelectedWorkItemIssue { get; set; }

        public void Publish ()
        {
            TotalItemsCount = WorkItemIssues.Count;
            var savedIssues = WorkItemIssues.Where(issue => issue.Save()).ToList();
            PublishedItemsCount = savedIssues.Count;
            // Clean the saved issues from the list
            foreach (IWorkItemIssue savedIssue in savedIssues)
            {
                WorkItemIssues.Remove(savedIssue);
            }
            // Trigger a refresh in the UI by raising a change notification. We could raise single notifications.
            // For ease of use we raise a reset
            OnPropertyChanged(string.Empty);
        }

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged (string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged (this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
