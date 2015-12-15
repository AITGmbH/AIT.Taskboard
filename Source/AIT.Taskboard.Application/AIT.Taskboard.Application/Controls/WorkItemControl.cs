using System;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.Application.UIInteraction;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.Application.Controls
{
    public class WorkItemControl : Thumb
    {
        public static readonly DependencyProperty IsBacklogItemProperty = DependencyProperty.Register("IsBacklogItem", typeof(bool), typeof(WorkItemControl));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(WorkItemControl), new PropertyMetadata(new PropertyChangedCallback(IsSelectedChanged)));
        
        private readonly TouchEventManager _mtm;
        private readonly IControlManipulation _manipulator;

        public WorkItemControl()
        {
            System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.Critical;


            _manipulator = new WorkitemControlManipulator(this);
            _mtm = new TouchEventManager(this, _manipulator);

            TouchDown += OnTouchDown;
            TouchUp += OnTouchUp;
            TouchMove += OnTouchMove;
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

        public bool IsBacklogItem
        {
            get { return (bool)GetValue(IsBacklogItemProperty); }
            set { SetValue(IsBacklogItemProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static WorkItemControl SelectedItem { get; set; }

        private ApplicationViewModel Model
        {
            get
            {
                var window = WpfHelper.FindVisualParent<Window>(this);
                if (window != null)
                {
                    return window.DataContext as ApplicationViewModel;
                }
                return null;
            }
        }

        private static void IsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                var control = sender as WorkItemControl;
                if (control != null && control.Model != null)
                {
                    control.Model.SelectedWorkItem = control.DataContext as WorkItem;
                    SelectedItem = control;
                }
            }
        }

        protected override void OnMouseDoubleClick(System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (!IsBacklogItem)
            {
                var window = WpfHelper.FindVisualParent<Window>(this);
                var taskboardControl = WpfHelper.FindVisualParent<TaskboardControl>(this);
                if (window != null)
                {
                    WindowHelper.ShowEditForm(window, Model.SelectedWorkItem);
                    if (IsBacklogItem)
                    {
                        WindowHelper.RefreshBacklogItemCell(taskboardControl);
                    }
                    else
                        WindowHelper.RefreshWorkItemCell(taskboardControl);
                }
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            Margin = new Thickness(4);

            // The following is required to ensure that after an item has been dropped it becomes selected again.
            if (Model.SelectedWorkItem == DataContext)
            {
                SelectedItem = this;
            }
        }
    }
}
