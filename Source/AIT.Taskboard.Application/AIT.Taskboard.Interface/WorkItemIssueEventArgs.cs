using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public class WorkItemIssueEventArgs : EventArgs
    {
        private List<IWorkItemIssue> _workItemIssues = new List<IWorkItemIssue>();
        public List<IWorkItemIssue> WorkItemIssues
        {
            get { return _workItemIssues; }
        }
    }
}