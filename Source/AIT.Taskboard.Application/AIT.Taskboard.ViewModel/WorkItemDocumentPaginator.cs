using System.Collections.Generic;
using System.Windows.Documents;
using System.Windows;
using System.Windows.Controls;

using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Windows.Media;
using System;
using AIT.Taskboard.Interface;

namespace AIT.Taskboard.ViewModel
{
    class WorkItemDocumentPaginator : DocumentPaginator
    {
        #region Constants
        
        private const double MmToinch = 0.0393700787402;
        private readonly Thickness PageMargin = new Thickness((15 * MmToinch) * 96, 
                                                              (15 * MmToinch) * 96, 
                                                              (15 * MmToinch) * 96, 
                                                              (15 * MmToinch) * 96);

        #endregion Constants

        #region Fields

        private ApplicationViewModel _applicationViewModel;
        private IList<WorkItem> _workItems;

        #endregion Fields

        #region Constructor

        public WorkItemDocumentPaginator(ApplicationViewModel applicationViewModel, IList<WorkItem> workItems)
        {
            _applicationViewModel = applicationViewModel;
            _workItems = workItems;
        }

        #endregion Constructor

        #region Override methods

        public override bool IsPageCountValid
        {
            get { return _workItems.Count > 0; }
        }

        public override int PageCount
        {
            get { return _workItems.Count; }
        }

        public override Size PageSize
        {
            get; set;
        }

        public override DocumentPage GetPage(int pageNumber)
        {
            // get work item for page
            var workItem = _workItems[pageNumber];

            // create control with data template
            var container = new ContentControl();
            container.Content = workItem;
            container.ContentTemplate = _applicationViewModel.WorkItemTemplateProvider.GetWorkItemTemplate(workItem.Type, _applicationViewModel.SelectedWorkItemSize);
            var paperSize = new Size(PageSize.Width - (PageMargin.Left + PageMargin.Right),
                                     PageSize.Height - (PageMargin.Top + PageMargin.Bottom));
            var workItemSize = _applicationViewModel.WorkItemSize;

            // scale whole work item control
            var scale = Math.Min(paperSize.Width / workItemSize.Width,
                                 paperSize.Height / workItemSize.Height);   
            container.LayoutTransform = new ScaleTransform(scale, scale);

            // resize it to page size
            container.Measure(paperSize);
            var containerSize = container.DesiredSize;
            var center = new Point((PageSize.Width - containerSize.Width) / 2,
                                   (PageSize.Height - containerSize.Height) / 2);
            container.Arrange(new System.Windows.Rect(center, containerSize));

            // return document with visual to print
            return new DocumentPage(container);
        }

        public override IDocumentPaginatorSource Source
        {
            get { return null; }
        }

        #endregion Override methods

    }
}
