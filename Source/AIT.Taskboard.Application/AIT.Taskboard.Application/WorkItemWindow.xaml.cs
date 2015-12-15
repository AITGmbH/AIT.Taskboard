using System;
using System.Collections;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for WorkItemWindow.xaml
    /// </summary>
    public partial class WorkItemWindow
    {
        #region Constructor

        public WorkItemWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        public WorkItem WorkItem
        {
            get { return (WorkItem)GetValue(WorkItemProperty); }
            set { SetValue(WorkItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WorkItemProperty =
            DependencyProperty.Register("WorkItem", typeof(WorkItem), typeof(WorkItemWindow), new PropertyMetadata(HandleWorkItemChanged));

        public ArrayList ValidationErrors
        {
            get { return (ArrayList)GetValue(ValidationErrorsProperty); }
            set { SetValue(ValidationErrorsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ValidationErrors.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationErrorsProperty =
            DependencyProperty.Register("ValidationErrors", typeof(ArrayList), typeof(WorkItemWindow) );

        public WorkItem ParentWorkItem { get; set; }

        public WorkItemLinkTypeEnd LinkType { get; set; }

        private bool IsClosed { get; set; }

        private bool IsRunningClose { get; set; }

        private bool IsClosing { get; set; }

        #endregion

        #region Methods

        private static void HandleWorkItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WorkItemWindow window = d as WorkItemWindow;
            if (window == null) return;
            window.DataContext = window;
            var item = e.NewValue as WorkItem;
            if (item == null) return;

            window.ValidationErrors = item.Validate();
            item.FieldChanged += (sender, args) => window.Dispatcher.Invoke(new Action<WorkItemWindow, ArrayList>(SetValidationErrors), window, item.Validate());
        }

        private static void SetValidationErrors(WorkItemWindow window, ArrayList validationErrors)
        {
            window.ValidationErrors = validationErrors;
            CommandManager.InvalidateRequerySuggested();
        }

        private void ExecuteClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (IsClosed) return;
            if (IsRunningClose) return;

            IsRunningClose = true;
            ValidationErrors = WorkItem.Validate();
            if (ValidationErrors.Count == 0)
            {
                IsClosed = true;
                if (!IsClosing)
                    Close();
            }
            else
            {
                var result = MessageBox.Show(this, Properties.Resources.WorkItemWindowValidationErrorOnCloseMessage,
                                             Properties.Resources.WorkItemWindowValidationErrorOnCloseTitle,
                                             MessageBoxButton.YesNo, MessageBoxImage.Exclamation, MessageBoxResult.No);
                // If the user wanted to close without fixing the error just close, otherwise do nothing
                if (result == MessageBoxResult.Yes)
                {
                    IsClosed = true;
                    if (!IsClosing)
                        Close();
                }
            }
            IsRunningClose = false;

            // Discard changes on parent work item
            if (ParentWorkItem != null)
                ParentWorkItem.Reset();
        }

        private void HandleWindowClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            ExecuteClose(this, null);
            e.Cancel = !IsClosed;
            IsClosing = false;
        }

        private void CanExecuteRefresh(object sender, CanExecuteRoutedEventArgs e)
        {
            // We only allow refresh when the work item has changes
            e.CanExecute = WorkItem != null && WorkItem.IsDirty;
        }

        private void ExecuteRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            if (WorkItem == null) return;
            if (!WorkItem.IsDirty) return;
            var result = MessageBox.Show(this,
                                         Properties.Resources.RefreshWorkItemMessage,
                                         Properties.Resources.RefreshWorkItemTitle, MessageBoxButton.YesNo, MessageBoxImage.Question,
                                         MessageBoxResult.No);
            if (result == MessageBoxResult.Yes)
            {
                WorkItem.Reset();
            }
        }      

        private void CanExecuteSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = WorkItem != null && WorkItem.IsDirty && WorkItem.IsValid();
        }

        private void ExecuteSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (WorkItem == null) return;
            if (!WorkItem.IsDirty) return;
            if (!WorkItem.IsValid()) return;
            WorkItem.Save();

            if (ParentWorkItem != null && LinkType != null)
            {
                ParentWorkItem.Links.Add(new RelatedLink(LinkType, WorkItem.Id));
                ParentWorkItem.Save();
            }
        }

        private void CanExecuteSaveAndClose(object sender, CanExecuteRoutedEventArgs e)
        {
            CanExecuteSave(sender, e);
        }

        private void ExecuteSaveAndClose(object sender, ExecutedRoutedEventArgs e)
        {
            ExecuteSave(sender, e);
            ExecuteClose(sender, e);
        }

        #endregion
    }
}
