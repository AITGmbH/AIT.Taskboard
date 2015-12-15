using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AIT.Taskboard.Application.Helper;
using AIT.Taskboard.ViewModel;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for WorkItemValidationWindow.xaml
    /// </summary>
    public partial class WorkItemIssueWindow : Window
    {
        public WorkItemIssueWindow()
        {
            InitializeComponent();
        }
        internal TaskboardControl TaskboardControl { get; set; }
        internal ApplicationViewModel ApplicationViewModel { get; set; }

        private void CanExecuteEditWorkItem(object sender, CanExecuteRoutedEventArgs e)
        {
            // TODO: Only allow execution if there is a selected work item
            e.CanExecute = Model != null && Model.SelectedWorkItemIssue != null;
            
        }

        private void ExecuteEditWorkItem(object sender, ExecutedRoutedEventArgs e)
        {
            // TODO: Open Work Item Form
            WindowHelper.ShowEditForm(this, Model.SelectedWorkItem);
            WindowHelper.RefreshWorkItemCell(TaskboardControl);
            Model.SelectedWorkItemIssue.Verify();
        }

        private void CanExecutePublishWorkItem(object sender, CanExecuteRoutedEventArgs e)
        {
            // Only allow publishing if there is at least one item that's ready for publishing
            e.CanExecute = Model != null && Model.CanPublish;
            
        }

        private void ExecutePublishWorkItem(object sender, ExecutedRoutedEventArgs e)
        {
            Model.Publish();
        }

        public WorkItemIssueViewModel Model { get; set; }

        private void ExecuteCancelIssueDialog(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        private void HandlePreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                TaskboardCommands.EditWorkItem.Execute(null, sender as IInputElement);
            }

        }

        private void HandleUnpublishedItemsKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Return))
                TaskboardCommands.EditWorkItem.Execute(null, sender as IInputElement);
        }
    }
}
