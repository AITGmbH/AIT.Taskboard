using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using AIT.Taskboard.Interface;
using AIT.Taskboard.Model.Properties;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Model
{
    class WorkItemIssue : IWorkItemIssue, INotifyPropertyChanged
    {
        #region Implementation of IWorkItemIssue

        public WorkItem WorkItem
        {
            get; set;
        }

        public string Issue
        {
            get
            {
                if (SaveFailed) return SaveFailure;
                return HasValidationErrors ? "Validation Failed" : string.Empty;
            }
        }

        public string Status
        {
            get
            {
                // When there are no changes the item is considered to be published
                // Otherwise the item is ready to publish if there are changes but no validation errors
                // Otherwise the item requires validation and is therefore not published
                if ((!HasValidationErrors) && (IsNewOrDirty)) return Resources.WorkItemIssueStatusReadyToPublish;
                if (IsNewOrDirty) return Resources.WorkItemIssueStatusNotPublished;
                return Resources.WorkItemIssueStatusNoChangesToPublish;
            }
        }

        public string DetailedIssue
        {
            get
            {
                if (!HasValidationErrors) return string.Empty;
                if (ValidationMessages == null) return string.Empty;
                if (ValidationMessages.Count == 0) return string.Empty;
                return GetDetailedMessage (ValidationMessages);
            }
        }
        public string GetDetailedMessage (ArrayList validationMessages)
        {
            string detailedMessage = string.Empty;
            foreach (var validationMessage in validationMessages)
            {
                var field = validationMessage as Field;
                if (field != null)
                {
                    var message = string.Format(Resources.WorkItemIssueDetailInvalidField, field.Name, field.Status);
                    if (string.IsNullOrEmpty(detailedMessage))
                        detailedMessage += message;
                    else
                    {
                        detailedMessage += (Environment.NewLine + message);
                    }
                }
            }
            return detailedMessage;
        }
        
        public bool HasValidationErrors
        {
            get
            {
                ValidationMessages = WorkItem.Validate();
                if (ValidationMessages == null) return false;
                if (ValidationMessages.Count == 0) return false;
                return true;
            }
        }

        public ArrayList ValidationMessages { get; private set; }

        public bool Save()
        {
            try
            {
                if (!HasValidationErrors)
                {
                    WorkItem.Save();
                    return true;
                }
                
            }
            catch (Exception e)
            {
                SaveFailed = true;
                SaveFailure = e.Message;
            }
            OnPropertyChanged(string.Empty);
            return false;
        }

        public bool CanPublish
        {
            get
            {
                // if the item has changes and no validation errors than it is ready for publishing
                return IsNewOrDirty && !HasValidationErrors;
            }
        }

        public void Verify()
        {
            OnPropertyChanged(string.Empty);
        }

        #endregion

        private string SaveFailure { get; set; }
        private bool SaveFailed { get; set; }
        private bool IsNewOrDirty
        {
            get
            {
                return WorkItem.IsDirty || WorkItem.IsNew;
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
