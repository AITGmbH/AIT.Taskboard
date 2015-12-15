using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows;

namespace AIT.Taskboard.Application.Helper
{
    internal static class ChildStateHelper
    {
        
        public static IEnumerable<WorkItem> GetWorkItemsByState(this IList<WorkItem> workItems, ICustomState state)
        {
            return workItems.Where(c => state.WorkItemStates.Contains(c.State));
        }

        
    }
}
