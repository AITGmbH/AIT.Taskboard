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
using AIT.Taskboard.ViewModel;
using System.ComponentModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for QuerySelectWindow.xaml
    /// </summary>
    public partial class QuerySelectWindow : Window
    {
        public QuerySelectWindow()
        {
            InitializeComponent();
        }

        TreeView QueryTreeView;

        ConfigurationViewModel _crossThreadModel;
        internal ConfigurationViewModel Model
        {
            get { return DataContext as ConfigurationViewModel; }
            set { DataContext = value; _crossThreadModel = value; }
        }

        private void CanExecuteCommitConfigurationDialog(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Model != null && Model.QueryItem != null;
        }

        private void ExecuteCommitConfigurationDialog(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void ExecuteCancelConfigurationDialog(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = false;
            Close();
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

        private void HandleQueryTreeSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
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
                             queryDef.QueryType == QueryType.OneHop))
                    {
                        try
                        {
                            viewModel.SetQueryItem(newQueryItem);
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
    }
}
