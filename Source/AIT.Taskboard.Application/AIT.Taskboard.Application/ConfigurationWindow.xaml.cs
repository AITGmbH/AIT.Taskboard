using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : Window
    {
        private TreeView QueryTreeView { get; set; }

        public ConfigurationWindow()
        {
            InitializeComponent();
        }

        ConfigurationViewModel _crossThreadModel;
        internal ConfigurationViewModel Model
        {
            get { return DataContext as ConfigurationViewModel; }
            set { DataContext = value; _crossThreadModel = value; }
        }

        #region Command Handling
        private void ExecuteSelectBacklogItem(object sender, ExecutedRoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked.Value)
                {
                    Model.BacklogItems.Add(((WorkItemType)e.Parameter).Name);
                }
                else
                {
                    Model.BacklogItems.Remove(((WorkItemType)e.Parameter).Name);
                }
            }
        }

        private void CanExecuteSelectBacklogItem(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((Model != null) && (Model.RestrictWorkItemTypeUsage))
                e.CanExecute = !Model.ChildItems.Contains(((WorkItemType)e.Parameter).Name);
            else
                e.CanExecute = Model != null;
        }

        private void ExecuteSelectChildItem(object sender, ExecutedRoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked.Value)
                {
                    Model.ChildItems.Add(((WorkItemType)e.Parameter).Name);
                }
                else
                {
                    Model.ChildItems.Remove(((WorkItemType)e.Parameter).Name);
                }
                Model.LoadWorkItemStates();
            }
        }

        private void CanExecuteSelectChildItem(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((Model != null) && (Model.RestrictWorkItemTypeUsage))
                e.CanExecute = !Model.BacklogItems.Contains(((WorkItemType)e.Parameter).Name);
            else
                e.CanExecute = Model != null;
        }

        private void CanExecuteAddCustomState(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && !String.IsNullOrEmpty(Model.TeamProjectName);
        }

        private void ExecuteAddCustomState(object sender, ExecutedRoutedEventArgs e)
        {
            Model.AddCustomState(Properties.Resources.NewCustomState);
        }

        private void ExecuteRemoveCustomState(object sender, ExecutedRoutedEventArgs e)
        {
            Model.RemoveCurrentCustomState();
        }

        private void CanExecuteRemoveCustomState(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.CurrentCustomState != null;
        }

        private void ExecuteMoveStateUp(object sender, ExecutedRoutedEventArgs e)
        {
            var index = Model.CustomStates.IndexOf(Model.CurrentCustomState);
            Model.CustomStates.Move(index, index - 1);
        }

        private void CanExecuteMoveStateUp(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.CurrentCustomState != null && Model.CustomStates.IndexOf(Model.CurrentCustomState) > 0;
        }

        private void ExecuteMoveStateDown(object sender, ExecutedRoutedEventArgs e)
        {
            var index = Model.CustomStates.IndexOf(Model.CurrentCustomState);
            Model.CustomStates.Move(index, index + 1);
        }

        private void CanExecuteMoveStateDown(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.CurrentCustomState != null && Model.CustomStates.IndexOf(Model.CurrentCustomState) < (Model.CustomStates.Count - 1);
        }

        private void ExecuteSelectWorkItemState(object sender, ExecutedRoutedEventArgs e)
        {
            var checkBox = e.OriginalSource as CheckBox;
            if (checkBox != null)
            {
                if (checkBox.IsChecked.Value)
                {
                    Model.CurrentCustomState.WorkItemStates.Add((string)e.Parameter);
                }
                else
                {
                    Model.CurrentCustomState.WorkItemStates.Remove((string)e.Parameter);
                }
            }
        }

        private void CanExecuteSelectWorkItemState(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model.CurrentCustomState != null && Model.CustomStates.Where(cs => cs != Model.CurrentCustomState && cs.WorkItemStates.Contains((string)e.Parameter)).Count() == 0;
        }

        private void CanExecuteCommitConfigurationDialog(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && !String.IsNullOrEmpty(Model.TeamProjectName);
        }

        private void ExecuteCommitConfigurationDialog(object sender, ExecutedRoutedEventArgs e)
        {
            //this cannot happen here, because it will block the UI thread
            //if instead we try to run it on a background thread, the main window will immediately try to read the new configuration and we will have concurrency issues
            //Model.Commit();

            DialogResult = true;
            Close();
        }

        private void ExecuteCancelConfigurationDialog(object sender, ExecutedRoutedEventArgs e)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += new DoWorkEventHandler(ResetDoWork);
            bw.RunWorkerAsync();

            DialogResult = false;
            Close();
        }

        private void ResetDoWork(object sender, DoWorkEventArgs e)
        {
            _crossThreadModel.Reset();      
        }
        #endregion

        #region Event Handling
        private void HandleQueryTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (queryItems.IsDropDownOpen)
            {
                var viewModel = DataContext as ConfigurationViewModel;
                if (viewModel != null)
                {
                    //look for the selected item in the original collection
                    QueryItem newQueryItem = viewModel.GetQueryItem(e.NewValue);
                    if (newQueryItem != null)
                    {
                        var queryDef = newQueryItem as QueryDefinition;
                        if (null != queryDef && 
                            (queryDef.QueryType == QueryType.Tree ||
                             queryDef.QueryType == QueryType.OneHop)
                            )
                            try
                            {
                                viewModel.SetQueryItem(newQueryItem);
                                queryItems.IsDropDownOpen = false;
                            }
                            catch
                            {
                                UnSelectItem(sender, newQueryItem, true);
                            }
                    }
                    else
                    {
                        UnSelectItem(sender, newQueryItem, false);
                    }
                }
            }
        }

        private void UnSelectItem(object sender, QueryItem newQueryItem, bool alsoDisable)
        {
            TreeView tree = sender as TreeView;
            TreeViewItem item = GetSelectedItem(tree.ItemContainerGenerator, newQueryItem);
            if (null != item)
            {
                QueryTreeView.SelectedItemChanged -= HandleQueryTreeSelectionChanged;
                item.IsSelected = false;
                if (alsoDisable)
                    item.IsEnabled = false;
                QueryTreeView.SelectedItemChanged += HandleQueryTreeSelectionChanged;
            }
        }

        private TreeViewItem GetSelectedItem(ItemContainerGenerator icg, QueryItem newQueryItem)
        {
            int i = 0;
            while (true)
            {
                TreeViewItem item = (TreeViewItem)icg.ContainerFromIndex(i++);
                if (item == null)
                    return null;
                if (item.Header.ToString().Equals(newQueryItem.Path, StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
                if (newQueryItem.Path.StartsWith(item.Header.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return GetSelectedItem(item.ItemContainerGenerator, newQueryItem);
                }                   
            }
        }

        private void treeView_Initialized(object sender, System.EventArgs e)
        {
            QueryTreeView = sender as TreeView;
            //if we have a query item selected, we need to expand the tree to this item
            if (Model != null && Model.QueryItem != null)
                QueryTreeView.ItemContainerGenerator.StatusChanged += new System.EventHandler(ItemContainerGenerator_StatusChanged);
        }
        
        void ItemContainerGenerator_StatusChanged(object sender, System.EventArgs e)
        {
            ItemContainerGenerator icg = sender as ItemContainerGenerator;
            if (icg.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                int i = 0;
                while (true)//we will eventually find the selected item, or we will get a null and exit the loop
                {
                    TreeViewItem item = (TreeViewItem)icg.ContainerFromIndex(i++);
                    if (item == null)
                        break;
                    if (Model.QueryItem.Path.StartsWith(item.Header.ToString()))
                    {
                        item.ItemContainerGenerator.StatusChanged += new System.EventHandler(ItemContainerGenerator_StatusChanged);
                        item.IsExpanded = true;
                        if (Model.QueryItem.Path == item.Header.ToString())
                        {
                            QueryTreeView.SelectedItemChanged -= HandleQueryTreeSelectionChanged;
                            item.IsSelected = true;
                            QueryTreeView.SelectedItemChanged += HandleQueryTreeSelectionChanged;
                        }
                        break;
                    }
                }
            }
        }

        private void selectProjectCollection_Click(object sender, RoutedEventArgs e)
        {
            Model.SelectTeamProject();
            if (!String.IsNullOrEmpty(Model.TeamProjectName))
            {
                SetProjectDependantControlsEnabled(true);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetProjectDependantControlsEnabled(!String.IsNullOrEmpty(Model.TeamProjectName));
            autoRefreshDelay.Value = autoRefreshDelay.Minimum;
        }
        #endregion

        private void SetProjectDependantControlsEnabled(bool enabled)
        {
            lvBacklogItems.IsEnabled = lvChildItems.IsEnabled = lvCustomStates.IsEnabled = lvWorkItemStates.IsEnabled = enabled;
            queryItems.IsEnabled = linkTypes.IsEnabled = itemSorting.IsEnabled = sortDirection.IsEnabled = rowSummary.IsEnabled = columnSummary.IsEnabled = enabled;
        }
    }
}