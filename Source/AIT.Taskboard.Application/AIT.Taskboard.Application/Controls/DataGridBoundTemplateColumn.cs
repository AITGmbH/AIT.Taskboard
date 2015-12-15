using System.Windows;
using System.Windows.Controls;

namespace AIT.Taskboard.Application.Controls
{
    /// <summary>
    /// Data grid bound templateable column
    /// </summary>
    public class DataGridBoundTemplateColumn : DataGridBoundColumn
    {
        /// <summary>
        /// Is column selected property
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(DataGridBoundTemplateColumn));

        /// <summary>
        /// Template property
        /// </summary>
        public static readonly DependencyProperty TemplateProperty = DependencyProperty.Register("Template", typeof(string), typeof(DataGridBoundTemplateColumn), new PropertyMetadata("CellDataTemplate"));

        /// <summary>
        /// Is column selected
        /// </summary>
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        /// <summary>
        /// Template
        /// </summary>
        public string Template
        {
            get
            {
                return (string)GetValue(TemplateProperty);
            }
            set
            {
                SetValue(TemplateProperty, value);
            }
        }

        /// <summary>
        /// Generates editing element
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            return GenerateElement(cell, dataItem);
        }

        /// <summary>
        /// Generate element
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="dataItem"></param>
        /// <returns></returns>
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            // Note: We do not set the DataContext as it is provided by the DataGridRow that is a parent 
            var contentControl = new ContentControl();
            contentControl.SetBinding(ContentControl.ContentProperty, Binding);
            contentControl.ContentTemplate = DataGridOwner.TryFindResource(Template) as DataTemplate;
            return contentControl;
        }
    }
}
