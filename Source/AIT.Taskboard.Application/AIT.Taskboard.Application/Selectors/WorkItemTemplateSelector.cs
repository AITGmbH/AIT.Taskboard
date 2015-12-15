using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Application.Controls;

namespace AIT.Taskboard.Application.Selectors
{
    public class WorkItemTemplateSelector : DataTemplateSelector
    {
        private ApplicationViewModel _applicationViewModel;

        public WorkItemTemplateSelector(FrameworkElement frameworkElement)
        {
            _applicationViewModel = GetViewModel(frameworkElement);
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var workItem = item as WorkItem;
            if (workItem != null)
            {
                // We expect a FrameworkElement aas container. Check it.
                var frameworkElement = (FrameworkElement) container;
                if (frameworkElement == null) return null;
                
                // The ApplicationViewModel should have been retrieved in the constructor
                if (_applicationViewModel != null)
                {
                    // Use the work item template provider to get the template for the work item
                    var template = _applicationViewModel.WorkItemTemplateProvider.GetWorkItemTemplate(workItem.Type, _applicationViewModel.SelectedWorkItemSize);
                    if (template != null)
                    {
                        return template;
                    }    
                }
                
                var workItemControl = WpfHelper.FindVisualParent<WorkItemControl>(container);
                if (workItemControl != null)
                {
                    string templateName = String.Format("Default{0}{1}WorkItemTemplate",
                        _applicationViewModel.SelectedWorkItemSize != Interface.WorkItemSize.Medium ? _applicationViewModel.SelectedWorkItemSize.ToString() : "",
                        workItemControl.IsBacklogItem ? "PBI" : "Child");

                    return (DataTemplate)frameworkElement.TryFindResource(templateName);
                }
            }
            return base.SelectTemplate(item, container);
        }

        private ApplicationViewModel GetViewModel(FrameworkElement frameworkElement)
        {
            if (frameworkElement ==null) return null;
            // CHeck if our data context is an ApplicationViewModel. If it is we just retrurn it.
            var model = frameworkElement.DataContext as ApplicationViewModel;
            if (model != null) return model;
            // If  our DataContext is no ApplicationViewModel we will traverse the visual tree up to the root 
            // and check if we find a parent that has the ApplicationViewModel set as DataContext
            if (frameworkElement.Parent != null) return GetViewModel(frameworkElement.Parent as FrameworkElement);
            return null;    
        }
    }
}
