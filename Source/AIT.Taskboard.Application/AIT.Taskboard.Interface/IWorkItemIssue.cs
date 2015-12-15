using System.Collections;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public interface IWorkItemIssue
    {
        WorkItem WorkItem { get; }
        string Issue { get; }
        string Status { get; }
        bool HasValidationErrors { get; }
        bool Save();
        bool CanPublish { get; }
        void Verify();
        string DetailedIssue { get; }
        ArrayList ValidationMessages { get; }
    }
}