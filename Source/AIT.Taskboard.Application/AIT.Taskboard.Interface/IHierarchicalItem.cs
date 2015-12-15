using System.Collections.ObjectModel;
using Microsoft.TeamFoundation.Client.Reporting;


namespace AIT.Taskboard.Interface
{
    public interface IHierarchicalItem
    {
        IHierarchicalItem Parent { get; set; }
        CatalogItem Item { get; }
        ObservableCollection<IHierarchicalItem> Children { get; }
    }
}