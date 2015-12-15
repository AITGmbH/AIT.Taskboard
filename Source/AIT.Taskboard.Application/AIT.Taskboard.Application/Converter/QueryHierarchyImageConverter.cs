using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application.Converter
{
    public sealed class QueryHierarchyImageConverter : IMultiValueConverter
    {
        private const string TeamExplorerFlatList = "SmallImages\\TeamExplorerFlatList.png";
        private const string TeamExplorerMyQueries = "SmallImages\\TeamExplorerMyQueries.png";
        private const string TeamExplorerDirectLink = "SmallImages\\TeamExplorerDirectLink.png";
        private const string TeamExplorerFolderCollapsed = "SmallImages\\TeamExplorerFolderCollapsed.png";
        private const string TeamExplorerFolderExpanded = "SmallImages\\TeamExplorerFolderExpanded.png";
        private const string TeamExplorerNoWorkItems = "SmallImages\\TeamExplorerNoWorkItems.png";
        private const string TeamExplorerTeamProject = "SmallImages\\TeamExplorerTeamProject.png";
        private const string TeamExplorerTeamQueries = "SmallImages\\TeamExplorerTeamQueries.png";
        private const string TeamExplorerTree = "SmallImages\\TeamExplorerTree.png";
        private const string TeamExplorerUser = "SmallImages\\TeamExplorerUser.png";
        private const string TeamExplorerWorkItemsRoot = "SmallImages\\TeamExplorerWorkItemsRoot.png";
        private const string Error = "SmallImages\\Error.png";

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return DependencyProperty.UnsetValue;
            if (values.Length < 2) return DependencyProperty.UnsetValue;

            QueryFolder folder = values[0] as QueryFolder;
            QueryHierarchy hierarchy = values[0] as QueryHierarchy;
            QueryDefinition definition = values[0] as QueryDefinition;

            string imagePath = TeamExplorerFolderCollapsed;
                

            bool isExpandend = values[1] is bool ? (bool) values[1] : false;

            if (definition != null)
            {
                switch (definition.QueryType)
                {
                    case QueryType.Invalid:
                        imagePath = TeamExplorerNoWorkItems;
                        break;
                    case QueryType.List:
                        imagePath = TeamExplorerFlatList;
                        break;
                    case QueryType.OneHop:
                        imagePath = TeamExplorerDirectLink;
                        break;
                    case QueryType.Tree:
                        imagePath = TeamExplorerTree;
                        break;
                    default:
                        imagePath = Error;
                        break;
                }
            }
            else if (folder != null)
            {
                hierarchy = folder.Parent as QueryHierarchy;
                if (hierarchy != null)
                {
                    // Our parent is the hierarchy so we are either My Queries or Team Queries
                    imagePath = folder.IsPersonal ? TeamExplorerMyQueries : TeamExplorerTeamQueries;
                }
                else
                    imagePath = isExpandend ? TeamExplorerFolderExpanded : TeamExplorerFolderCollapsed;
            }
            else if (hierarchy != null)
            {
                imagePath = TeamExplorerWorkItemsRoot;
            }

            try
            {
                return new BitmapImage(new Uri(imagePath,UriKind.Relative));
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetType,
                                  object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
