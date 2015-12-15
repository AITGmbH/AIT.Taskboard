using System.Collections.Generic;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public interface IBacklogChildren
    {
        WorkItem Backlog { get; }
        List<WorkItem> Children { get; }
        List<ICustomState> States { get; }
    }
}