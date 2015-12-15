using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections;
using System.Windows.Documents;
using System.Reflection;
using System.Windows.Threading;
using AIT.Taskboard.Application.Controls;
using AIT.Taskboard.Application.UIInteraction;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Interface;
using AIT.Taskboard.ViewModel;

namespace AIT.Taskboard.Application.DragDrop
{
	public class DragDropHelper
	{
	    private const double DragAndDropOffset = 50;

		// source and target
		private DataFormat format = DataFormats.GetDataFormat("DragDropItemsControl");
		private Point initialMousePosition;
		private object draggedData;
		private DraggedAdorner draggedAdorner;
		private Window topWindow;
		// source
		private ItemsControl sourceItemsControl;
		private FrameworkElement sourceItemContainer;
		// target
		private ItemsControl targetItemsControl;
		// singleton
		private static DragDropHelper instance;
		private static DragDropHelper Instance 
		{
			get 
			{  
				if(instance == null)
				{
					instance = new DragDropHelper();
				}
				return instance;
			}
		}

		public static bool GetIsDragSource(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDragSourceProperty);
		}

		public static void SetIsDragSource(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDragSourceProperty, value);
		}

		public static readonly DependencyProperty IsDragSourceProperty =
			DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDragSourceChanged));


		public static bool GetIsDropTarget(DependencyObject obj)
		{
			return (bool)obj.GetValue(IsDropTargetProperty);
		}

		public static void SetIsDropTarget(DependencyObject obj, bool value)
		{
			obj.SetValue(IsDropTargetProperty, value);
		}

		public static readonly DependencyProperty IsDropTargetProperty =
			DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropHelper), new UIPropertyMetadata(false, IsDropTargetChanged));

		private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dragSource = obj as ItemsControl;
			if (dragSource != null)
			{
				if (Object.Equals(e.NewValue, true))
				{
                    dragSource.MouseDoubleClick += Instance.DragSource_MouseDoubleClick;
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
					dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
					dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
				}
				else
				{
                    dragSource.MouseDoubleClick -= Instance.DragSource_MouseDoubleClick;
					dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
					dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
					dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
				}
			}
		}

		private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var dropTarget = obj as ItemsControl;
			if (dropTarget != null)
			{
				if (Object.Equals(e.NewValue, true))
				{
					dropTarget.AllowDrop = true;
					dropTarget.PreviewDrop += Instance.DropTarget_PreviewDrop;
					dropTarget.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
					dropTarget.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
					dropTarget.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
				}
				else
				{
					dropTarget.AllowDrop = false;
					dropTarget.PreviewDrop -= Instance.DropTarget_PreviewDrop;
					dropTarget.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
					dropTarget.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
					dropTarget.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
				}
			}
		}

        #region DragSource

        private void DragSource_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.sourceItemsControl = null;
        }

        private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
            // Disable drag and drop while workitem zoom.
            if(TouchEventManager.IsZooming)
            {
                return;
            }

			this.sourceItemsControl = (ItemsControl)sender;

            var grid = WpfHelper.FindVisualParent<DataGrid>(sourceItemsControl);
            var scroller = WpfHelper.FindVisualChild<ScrollViewer>(grid);
 
			Visual visual = e.OriginalSource as Visual;

			this.topWindow = Window.GetWindow(this.sourceItemsControl);
			this.initialMousePosition = e.GetPosition(this.topWindow);
            //take into account the scroller position
            this.initialMousePosition.X += scroller.HorizontalOffset;

			this.sourceItemContainer = sourceItemsControl.ContainerFromElement(visual) as FrameworkElement;
			if (this.sourceItemContainer != null)
			{
				this.draggedData = this.sourceItemContainer.DataContext;
			}
		}

		private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
		{
            if (TouchEventManager.IsZooming) return;

			if (this.draggedData != null)
			{
				// Only drag when user moved the mouse by a reasonable amount.
				if (IsMovementBigEnough(this.initialMousePosition, e.GetPosition(this.topWindow)))
				{
					DataObject data = new DataObject(this.format.Name, this.draggedData);

					// Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
					bool previousAllowDrop = this.topWindow.AllowDrop;
					this.topWindow.AllowDrop = true;
					this.topWindow.DragEnter += TopWindow_DragEnter;
					this.topWindow.DragOver += TopWindow_DragOver;
					this.topWindow.DragLeave += TopWindow_DragLeave;

                    DragDropEffects effects;

                    // Disable Drag & Drop while zooming
                    if (TouchEventManager.IsZooming)
                    {
                        effects = System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.None);
                    }
                    else
                    {
                        effects = System.Windows.DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
                    }

					// Without this call, there would be a bug in the following scenario: Click on a data item, and drag
					// the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
					// the Window leave event, and the dragged adorner is left behind.
					// With this call, the dragged adorner will disappear when we release the mouse outside of the window,
					// which is when the DoDragDrop synchronous method returns.
					RemoveDraggedAdorner();

					this.topWindow.AllowDrop = previousAllowDrop;
					this.topWindow.DragEnter -= TopWindow_DragEnter;
					this.topWindow.DragOver -= TopWindow_DragOver;
					this.topWindow.DragLeave -= TopWindow_DragLeave;
					
					this.draggedData = null;
				}
			}
		}
			
		private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			this.draggedData = null;
		}

        #endregion

        #region DropTarget

        private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
		{
			this.targetItemsControl = (ItemsControl)sender;
			object draggedItem = e.Data.GetData(this.format.Name);

			DecideDropTarget(e);
			if (draggedItem != null)
			{
				// Dragged Adorner is created on the first enter only.
				ShowDraggedAdorner(e.GetPosition(this.topWindow));
			}
			e.Handled = true;
		}

		private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
		{
			object draggedItem = e.Data.GetData(this.format.Name);

			DecideDropTarget(e);
            if (draggedItem != null)
            {
                Point position = e.GetPosition(this.topWindow);

                var grid = WpfHelper.FindVisualParent<DataGrid>(targetItemsControl);
                var scroller = WpfHelper.FindVisualChild<ScrollViewer>(grid);

                if (e.GetPosition(scroller).X > (scroller.ViewportWidth - 20))
                {
                    scroller.LineRight();
                }
                else if (e.GetPosition(scroller).X < (WpfHelper.FindVisualChild<Button>(grid).ActualWidth + 20))
                {
                    scroller.LineLeft();
                }
                //take into account the scroller position
                position.X += scroller.HorizontalOffset;
                // Dragged Adorner is only updated here - it has already been created in DragEnter.
                ShowDraggedAdorner(position);
            }
			e.Handled = true;
		}

		private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
		{
			object draggedItem = e.Data.GetData(this.format.Name);
            var workItem = draggedItem as WorkItem;
            if (workItem != null)
			{
				if ((e.Effects & DragDropEffects.Move) != 0)
				{
                    var sourceCell = WpfHelper.FindVisualParent<DataGridCell>(sourceItemsControl);
                    var targetCell = WpfHelper.FindVisualParent<DataGridCell>(targetItemsControl);
                    var grid = WpfHelper.FindVisualParent<DataGrid>(targetItemsControl);
                    var taskboard = WpfHelper.FindVisualParent<TaskboardControl>(targetItemsControl);
                    if (sourceCell != null && targetCell != null && grid != null && taskboard != null)
                    {
                        var targetState = targetCell.Column.Header as ICustomState;
                        var model = grid.DataContext as ApplicationViewModel;
                        workItem.PartialOpen();
                        workItem.State = model.GetTargetState(workItem, targetState);

                        WindowHelper.ResetDataContext(sourceCell);
                        WindowHelper.ResetDataContext(targetCell);
                                              

                        model.RefreshSummaries();
                        // The following ensures that the input bar recognizes that the element has been moved.
                        model.SelectedWorkItem = model.SelectedWorkItem;
                    }
                }

                RemoveDraggedAdorner();
			}
			e.Handled = true;
		}

		private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
		{
			// Dragged Adorner is only created once on DragEnter + every time we enter the window. 
			// It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
			object draggedItem = e.Data.GetData(this.format.Name);

			e.Handled = true;
		}

		private void DecideDropTarget(DragEventArgs e)
		{
			object draggedItem = e.Data.GetData(this.format.Name);
            var workItem = draggedItem as WorkItem;
            if (workItem != null)
            {
                var cellTarget = WpfHelper.FindVisualParent<DataGridCell>(targetItemsControl);
                var cellSource = WpfHelper.FindVisualParent<DataGridCell>(sourceItemsControl);
                var rowTarget = WpfHelper.FindVisualParent<DataGridRow>(cellTarget);
                var rowSource = WpfHelper.FindVisualParent<DataGridRow>(cellSource);
                var grid = WpfHelper.FindVisualParent<DataGrid>(targetItemsControl);
                if (cellTarget != null && cellSource != null && grid != null)
                {
                    if (cellTarget != cellSource && rowTarget == rowSource)
                    {
                        var targetState = cellTarget.Column.Header as ICustomState;
                        var model = grid.DataContext as ApplicationViewModel;
                        string state = model.GetTargetState(workItem, targetState);
                        if (!String.IsNullOrEmpty(state))
                        {
                            return;
                        }
                    }
                }
            }
            e.Effects = DragDropEffects.None;
        }

        #endregion

        #region Window

        private void TopWindow_DragEnter(object sender, DragEventArgs e)
		{
			ShowDraggedAdorner(e.GetPosition(this.topWindow));
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragOver(object sender, DragEventArgs e)
		{
			ShowDraggedAdorner(e.GetPosition(this.topWindow));
			e.Effects = DragDropEffects.None;
			e.Handled = true;
		}

		private void TopWindow_DragLeave(object sender, DragEventArgs e)
		{
			RemoveDraggedAdorner();
			e.Handled = true;
		}

        #endregion

        #region Adorners

		private void ShowDraggedAdorner(Point currentPosition)
		{
			if (this.draggedAdorner == null)
			{
                if (this.sourceItemsControl == null)
                    return;
				var adornerLayer = AdornerLayer.GetAdornerLayer(this.sourceItemsControl);
                if (adornerLayer == null || this.draggedData == null)
                {
                    return;
                }
                this.draggedAdorner = new DraggedAdorner(this.draggedData as WorkItem, this.sourceItemContainer, adornerLayer);
            }
            this.draggedAdorner.SetPosition(currentPosition.X - this.initialMousePosition.X, currentPosition.Y - this.initialMousePosition.Y);
		}

		private void RemoveDraggedAdorner()
		{
			if (this.draggedAdorner != null)
			{
				this.draggedAdorner.Detach();
				this.draggedAdorner = null;
			}
		}

        private bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialMousePosition.X) >= DragAndDropOffset ||
                 Math.Abs(currentPosition.Y - initialMousePosition.Y) >= DragAndDropOffset);
        }

        #endregion
    }
}
