using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AIT.Taskboard.ViewModel;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for LinkedWorkItemWindow.xaml
    /// </summary>
    public partial class LinkedWorkItemWindow : Window
    {       
        public LinkedWorkItemWindow()
        {
            InitializeComponent();
        }

        LinkedWorkItemViewModel _crossThreadModel;
        internal LinkedWorkItemViewModel Model
        {
            get 
            {
                return DataContext as LinkedWorkItemViewModel; }

            set 
            { 
                DataContext = value; 
                _crossThreadModel = value; 
                if(_crossThreadModel.LinkTypeEnds.Count<WorkItemLinkTypeEnd>() > 0)
                {
                    _crossThreadModel.SelectedLinkTypeEnd = _crossThreadModel.LinkTypeEnds.First<WorkItemLinkTypeEnd>();
                }
                if (_crossThreadModel.WorkItemTypes.Count > 0)
                {
                    _crossThreadModel.SelectedWorkItemType = _crossThreadModel.WorkItemTypes[0];
                }
            }
        }

        private void linkTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //based on link type set preview visualization template
            ControlTemplate template = TryFindResource("ChildParentListPreviewControl") as ControlTemplate;
            switch (_crossThreadModel.SelectedLinkTypeEnd.Name)
            {
                case "Child":
                    template = TryFindResource("ChildParentTreePreviewControl") as ControlTemplate;
                    break;
                case "Parent":
                    template = TryFindResource("ParentChildTreePreviewControl") as ControlTemplate;
                    break;
                case "Predecessor":
                case "Shared Steps":
                case "Tests":
                    template = TryFindResource("ParentChildListPreviewControl") as ControlTemplate;
                    break;
                case "Related":
                    template = TryFindResource("ParentChildRelatedPreviewControl") as ControlTemplate;
                    break;
                case "Successor":
                case "Test Case":
                case "Tested By":
                    template = TryFindResource("ChildParentListPreviewControl") as ControlTemplate;
                    break;

            }
            previewVizualization.Template = template;
            previewVizualization.InvalidateVisual();

        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

    }
}
