using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using AIT.Taskboard.Application.Selectors;
using AIT.Taskboard.Application.Helper;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AIT.Taskboard.Application.Controls;

namespace AIT.Taskboard.Application.DragDrop
{
    public class DraggedAdorner : Adorner
    {
        private WorkItemControl contentPresenter;
        private double left;
        private double top;
        private AdornerLayer adornerLayer;

        public DraggedAdorner(WorkItem dragDropData, UIElement adornedElement, AdornerLayer adornerLayer)
            : base(adornedElement)
        {
            this.adornerLayer = adornerLayer;

            this.contentPresenter = new WorkItemControl();
            this.contentPresenter.DataContext = dragDropData;
            var item = WpfHelper.FindVisualParent<TaskboardControl>(AdornedElement);
            if (item != null)
                this.contentPresenter.Resources = item.Resources;
            this.contentPresenter.Opacity = 0.7;

            this.adornerLayer.Add(this);
        }

        public void SetPosition(double left, double top)
        {
            this.left = left;
            this.top = top;
            //check that the adorner layer has adorners before trying to update. Otherwise we will get an exception
            if (this.adornerLayer != null && this.adornerLayer.GetAdorners(this.AdornedElement) != null)
            {
                this.adornerLayer.Update(this.AdornedElement);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.contentPresenter.Measure(constraint);
            //return this.contentPresenter.DesiredSize;
            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.contentPresenter.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.contentPresenter;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(this.left, this.top));

            return result;
        }

        public void Detach()
        {
            this.adornerLayer.Remove(this);
        }

    }
}
