using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using AIT.Taskboard.Application.Controls;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Application.UIInteraction;
using AIT.Taskboard.Interface;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using DataGridColumn = System.Windows.Controls.DataGridColumn;
using DataGridLength = System.Windows.Controls.DataGridLength;
using DataGridLengthUnitType = System.Windows.Controls.DataGridLengthUnitType;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for TaskboardControl.xaml
    /// </summary>
    public partial class TaskboardControl : UserControl
    {
        public TaskboardControl()
        {
            InitializeComponent();
            InitializeMultiTouchSupport();
        }

        #region Multitouch Support

        private IControlManipulation _manipulator;
        private TouchEventManager _mtm;

        /// <summary>
        /// Initializes MultiTouchSupport for TaskboardControl
        /// </summary>
        private void InitializeMultiTouchSupport()
        {
            _manipulator = new TaskboardControlManipulator(this);
            _mtm = new TouchEventManager(this, _manipulator);
            TouchEventManager.WorkitemZoomEnded += new EventHandler(MultiTouchManager_WorkitemZoomended);
            TouchEventManager.TaskboardZoomEnded += new EventHandler(MultiTouchManager_WorkitemZoomended);

            TouchDown += OnTouchDown;
            TouchUp += OnTouchUp;
            TouchMove += OnTouchMove;
        }

        void MultiTouchManager_WorkitemZoomended(object sender, EventArgs e)
        {
            UpdateColumnWidth();
            FocusSelectedWorkitem();
        }

        /// <summary>
        /// Move the currently selected workitem into the visible part of the taskboardcontrol.
        /// </summary>
        private void FocusSelectedWorkitem()
        {
            IList<WorkItemControl> workitemList = WpfHelper.FindVisualChildren<WorkItemControl>(dataGrid);

            foreach (WorkItemControl workItemControl in workitemList)
            {
                WorkItem workItem = workItemControl.DataContext as WorkItem;

                if (workItem != null)
                {
                    if (workItem == Model.SelectedWorkItem)
                    {
                        workItemControl.BringIntoView();
                    }
                }
            }
        }

        /// <summary>
        /// Updates the grid columns.
        /// </summary>
        private void UpdateColumnWidth()
        {
            double size;

            foreach (DataGridColumn dataGridColumn in dataGrid.Columns)
            {
                ICustomState header = dataGridColumn.Header as ICustomState;

                if(header == null) continue;

                if(Model != null && Model.SelectedWorkItem != null)
                {
                    if(header.Name == Model.SelectedWorkItem.State)
                    {
                        size = Math.Max(GetLargestWorkitemWithState(Model.SelectedWorkItem.State), 200);
                        
                        if(dataGridColumn.Width.Value <= size + 21)
                        {
                            size = (size + 21) * ZoomFactor;
                            dataGridColumn.Width = new DataGridLength(size);
                        }

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the largest workitem size of all workitems associated to a specific workitem state.
        /// </summary>
        /// <param name="state">Workitem state.</param>
        /// <returns>Size of the largest workitem. Returns 0 if no workitem has been found with the given state.</returns>
        private double GetLargestWorkitemWithState(string state)
        {
            IList<WorkItemControl> workitemList = WpfHelper.FindVisualChildren<WorkItemControl>(dataGrid);
            double width = 0;

            foreach (WorkItemControl workItemControl in workitemList)
            {
                WorkItem workItem = workItemControl.DataContext as WorkItem;

                if(workItem != null)
                {
                    if(workItem.State == state)
                    {
                        width = Math.Max(width, workItemControl.Width * workItemControl.LayoutTransform.Value.M11);
                    }  
                }
            }

            return width;
        }
     
        /// <summary>
        /// Exposes the ZoomFactor for the MultiTouchManager
        /// </summary>
        public double ZoomFactor
        {
            get { return Model.ZoomFactor; }
            set { Model.ZoomFactor = value; }
        }

        private void OnTouchDown(object sender, TouchEventArgs e)
        {
            e.Handled = _mtm.ProcessTouchDown(e);
        }

        private void OnTouchUp(object sender, TouchEventArgs e)
        {
            e.Handled = _mtm.ProcessTouchUp(e);
        }

        private void OnTouchMove(object sender, TouchEventArgs e)
        {
            e.Handled = _mtm.ProcessTouchMove(e);
        }

        #region Taskboard Scrolling

        /// <summary>
        /// Enables TaskboardScrolling
        /// </summary>
        public void EnableTaskboardScrolling()
        {
            var scroller = GetScrollViewerFromDatagrid();
            if (scroller != null) scroller.PanningMode = PanningMode.Both;
        }

        /// <summary>
        /// Disables TaskboardScrolling
        /// </summary>
        public void DisableTaskboardScrolling()
        {
            var scroller = GetScrollViewerFromDatagrid();
            if (scroller != null) scroller.PanningMode = PanningMode.None;
        }

        /// <summary>
        /// Uses recursive FindVisualChild-method to get the Scrollviewer-Object from internal datagrid
        /// </summary>
        /// <returns>Scrollviewer of datagrid</returns>
        private ScrollViewer GetScrollViewerFromDatagrid()
        {
            return FindVisualChild<ScrollViewer>(dataGrid);
        }

        /// <summary>
        /// Recursive search for object type of childitem with obj as depencency and parent object
        /// </summary>
        /// <typeparam name="TChildItem">type of childitem</typeparam>
        /// <param name="obj">parentdependecnyobject</param>
        /// <returns>childitem of given type or null</returns>
        private static TChildItem FindVisualChild<TChildItem>(DependencyObject obj) where TChildItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is ScrollViewer)
                {
                    return (TChildItem)child;
                }
                else
                {
                    var childOfChild = FindVisualChild<TChildItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }

            return null;
        }

        #endregion

        #endregion

        private ApplicationViewModel Model
        {
            get { return DataContext as ApplicationViewModel; }
        }

        public void ClearGrid()
        {
            dataGrid.Columns.Clear();
        }

        public void RefreshGrid(bool resetColumnsWidth)
        {
            if (!Model.IsConnected)
                return;
            var columnWidths = dataGrid.Columns.Select(item => item.Width).ToArray();

            dataGrid.Columns.Clear();
            dataGrid.SelectedIndex = -1;
            Model.SelectedWorkItem = null;

            if (Model.CustomStates == null)
            {
                dataGrid.Items.Refresh();
                return;
            }

            if (Model.WorkItemTemplateProvider != null) //this should never be null
                //add 21 to the workitem size due to various margins and borders
                dataGrid.MinColumnWidth = Math.Max(dataGrid.MinColumnWidth, Model.WorkItemSize.Width + 21);
            else
                dataGrid.MinColumnWidth = dataGrid.RowHeaderActualWidth;

            for (int i = 0; i < Model.CustomStates.Count; i++)
            {
                var state = Model.CustomStates[i];
                var col = new DataGridBoundTemplateColumn();
                col.Template = "WorkItemCell";
                col.Header = state;
                if (resetColumnsWidth || columnWidths.Length <= i)
                    col.Width = new DataGridLength((dataGrid.ActualWidth - dataGrid.MinColumnWidth - 21) / Model.CustomStates.Count, DataGridLengthUnitType.Pixel);
                else
                    col.Width = columnWidths[i];
                var binding = new Binding("CustomState" + i) { Mode = BindingMode.OneWay };
                col.Binding = binding;
                dataGrid.Columns.Add(col);
            }

            dataGrid.Items.Refresh();
        }

        private void More_Click(object sender, RoutedEventArgs e)
        {
            var window = WpfHelper.FindVisualParent<Window>(this);
            if (window != null)
            {
                WindowHelper.ShowEditForm(window, Model.SelectedWorkItem);
                WindowHelper.RefreshWorkItemCell(this);
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if ((IsVisible))
            {
                try { RefreshGrid(true); }
                catch { }
            }
        }

        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var row = WpfHelper.FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
            if (row != null)
            {
                row.IsSelected = true;
                if ((e.OriginalSource as WorkItemControl) == null)
                {
                    var rowHeader = WpfHelper.FindVisualChild<System.Windows.Controls.Primitives.DataGridRowHeader>(row);
                    var backlogItem = WpfHelper.FindVisualChild<WorkItemControl>(rowHeader);
                    if (backlogItem != null)
                        backlogItem.Focus();
                }
            }
        }

        private void HandleManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = this;
            e.Mode = ManipulationModes.Scale;
        }

        private void HandleManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Model.ZoomFactor *= Math.Max(e.DeltaManipulation.Scale.X, e.DeltaManipulation.Scale.Y);
        }

        private void btnAddLinkedItem_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Control)(e.Source)).TemplatedParent as WorkItemControl;
            if (item != null) item.Focus();
            AddLinkedWorkItem();
        }

        public void AddLinkedWorkItem()
        {
            var window = WpfHelper.FindVisualParent<Window>(this);
            if (window != null)
            {
                LinkedWorkItemWindow wnd = new LinkedWorkItemWindow();
                Model.LinkedWorkItemViewModel.WorkItem = Model.SelectedWorkItem;
                wnd.Model = Model.LinkedWorkItemViewModel;
                wnd.Owner = window;
                Nullable<bool> dialogResult = wnd.ShowDialog();

                WorkItem newItem = null;
                if (dialogResult == true)
                {
                    //save work item
                    newItem = Model.CreateNewLinkedWorkItem();

                    if (!Model.IsVisibleNewLinkedWorkItem(newItem, Model.LinkedWorkItemViewModel.SelectedLinkTypeEnd))
                    {
                        WindowHelper.ShowEditForm(window, newItem, Model.SelectedWorkItem, Model.LinkedWorkItemViewModel.SelectedLinkTypeEnd);
                        WindowHelper.RefreshWorkItemCell(this);
                    }
                    else
                    {
                        var cursor = this.Cursor;
                        this.Cursor = Cursors.Wait;
                        Model.SaveLinkedItem(newItem);
                        try { RefreshGrid(true); }
                        catch { }
                        this.Cursor = cursor;
                    }
                }

                // clear view model data
                Model.LinkedWorkItemViewModel.ClearData();
            }
        }
    }
}
