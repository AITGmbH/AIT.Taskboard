using System;
using System.Collections.ObjectModel;
using AIT.Taskboard.Interface;
using Microsoft.TeamFoundation.Client.Reporting;
using System.ComponentModel.Composition;

namespace AIT.Taskboard.Model
{
    public class HierarchicalItem : IHierarchicalItem
    {
        private ObservableCollection<IHierarchicalItem> _children;

        internal HierarchicalItem(ReportingServiceProxy proxy, CatalogItem item)
        {
            Proxy = proxy;
            Item = item;
        }

        private ReportingServiceProxy Proxy { get; set; }
        [Import]
        private ILogger Logger { get; set; }

        public IHierarchicalItem Parent { get; set; }

        public CatalogItem Item { get; private set; }

        public ObservableCollection<IHierarchicalItem> Children
        {
            get
            {
                if (_children == null)
                {
                    _children = new ObservableCollection<IHierarchicalItem>();
                    if (Item.Type != ItemTypeEnum.Report)
                    {
                        CatalogItem[] children = SaveGetChildren();
                        foreach (var child in children)
                        {
                            var item = new HierarchicalItem(Proxy, child) {Parent = this};
                            _children.Add(item);
                        }
                    }
                }
                return _children;
            }
        }

        private CatalogItem[] SaveGetChildren()
        {
            try
            {
                return Proxy.ListChildren(Item.Path, false);
            }
            catch(Exception exception)
            {
                Logger.LogException(exception);
                return new CatalogItem[0];
            }
        }
    } 
}
