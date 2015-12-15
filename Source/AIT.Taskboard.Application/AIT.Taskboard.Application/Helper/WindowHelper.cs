using System.Windows;
using System.Windows.Controls;
using AIT.Taskboard.Application.Controls;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Controls;

namespace AIT.Taskboard.Application.Helper
{
    internal static class WindowHelper
    {
        internal static void ShowEditForm(Window owner, WorkItem workItem, WorkItem parentItem, WorkItemLinkTypeEnd linkType)
        {
            workItem.Open();
            var editForm = new WorkItemFormControl {Item = workItem, ReadOnly = false};
            var infoBar = new WorkItemInformationBar();
            var window = new WorkItemWindow
                             {
                                 wiFormHost = {Child = editForm},
                                 wiInfoBarHost = {Child = infoBar},
                                 WorkItem = workItem,
                                 ParentWorkItem = parentItem,
                                 LinkType = linkType,
                                 Owner = owner
                             };
            // Every time a work item field changes we need to update the info bar
            WorkItemFieldChangeEventHandler handleFieldChanged = delegate { infoBar.Refresh(editForm, string.Empty, string.Empty); };
            window.WorkItem.FieldChanged += handleFieldChanged;
            // Additionally we need to update the info bar also upon first start
            infoBar.Refresh(editForm, string.Empty, string.Empty);
            window.ShowDialog();
            // Unsubscribe form the event handler in order not to be notified anymore
            window.WorkItem.FieldChanged -= handleFieldChanged;

            window.Close();
        }

        internal static void ShowEditForm(Window owner, WorkItem workItem)
        {
            ShowEditForm(owner, workItem, null, null);
        }
        internal static void RefreshWorkItemCell(TaskboardControl taskboard)
        {
            var grid = WpfHelper.FindVisualChild<DataGrid>(taskboard);
            if (grid != null)
            {
                // get selected item
                var workItemControl = WorkItemControl.SelectedItem;
                if (workItemControl != null)
                {
                    // get cells presenter for current row
                    var gridCellsPresenter = WpfHelper.FindVisualParent<System.Windows.Controls.Primitives.DataGridCellsPresenter>(workItemControl);
                    if (gridCellsPresenter != null)
                    {
                        for (int i = 0; i < gridCellsPresenter.Items.Count; i++)
                        {
                            var cell = gridCellsPresenter.ItemContainerGenerator.ContainerFromIndex(i) as DataGridCell;
                            if (cell != null)
                            {
                                // reset data context for cell to re-render content
                                ResetDataContext(cell);
                            }
                        }
                    }
                }

                var applicationViewModel = grid.DataContext as ApplicationViewModel;
                if (applicationViewModel != null)
                {
                    applicationViewModel.RefreshSummaries();
                }
            }
        }
        internal static void ResetDataContext(FrameworkElement element)
        {
            if (element == null) return;
            var dc = element.DataContext;
            element.DataContext = null;
            element.DataContext = dc;
        }
        public static void RefreshBacklogItemCell(TaskboardControl taskboardControl)
        {
            if (taskboardControl != null)
            {
                var grid = WpfHelper.FindVisualChild<DataGrid>(taskboardControl);
                if (grid != null)
                {
                    var workItemControl = WorkItemControl.SelectedItem;
                    if (workItemControl != null)
                    {
                        var row = WpfHelper.FindVisualParent<DataGridRow>(workItemControl);
                        if (row != null)
                        {
                            ResetDataContext(row);
                        }
                        var childPresenter = WpfHelper.FindVisualChild<ContentPresenter>(workItemControl);
                        if (childPresenter != null)
                        {
                            ResetDataContext(childPresenter);
                        }
                    }
                }
            }
        }
        
    }
}
