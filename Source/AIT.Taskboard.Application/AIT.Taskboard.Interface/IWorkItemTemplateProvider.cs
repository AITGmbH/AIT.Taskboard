using System.Windows;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;

namespace AIT.Taskboard.Interface
{
    public interface IWorkItemTemplateProvider
    {
        DataTemplate GetWorkItemTemplate(WorkItemType workItemType, WorkItemSize size);
        string WorkItemTemplatesFile { get; set; }
        Size GetWorkItemSize(WorkItemSize workItemSize);
        bool IsSizeDefined(WorkItemSize workItemSize);
    }
}
