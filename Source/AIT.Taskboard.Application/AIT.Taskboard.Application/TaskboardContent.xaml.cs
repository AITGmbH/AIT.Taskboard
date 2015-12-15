using System;
using System.Windows;
using System.Windows.Controls;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for TaskboardContent.xaml
    /// </summary>
    public partial class TaskboardContent : UserControl
    {
        private double _currentSplitterPosition;

        public TaskboardContent()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            taskboardControl.ClearGrid();
        }

        public void ReBind(bool resetColumnsWidth)
        {
            taskboardControl.RefreshGrid(resetColumnsWidth);
        }

        public double SplitterPosition
        {
            get { return taskboardControl.ActualHeight == 0 ? Double.MaxValue : _currentSplitterPosition; }
            set
            {
                _currentSplitterPosition = value;

                if (reportControl.Visibility == Visibility.Visible)
                {
                    grid.RowDefinitions[0].Height = new GridLength(_currentSplitterPosition, GridUnitType.Star);
                    grid.RowDefinitions[1].Height = new GridLength(30);
                    grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    foreach (var rowDefinition in grid.RowDefinitions)
                    {
                        rowDefinition.Height = new GridLength(0, GridUnitType.Auto);
                    }
                }
            }
        }

        private void ReportControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetGridRowRowsHeight();
        }

        private void SetGridRowRowsHeight()
        {
            if (reportControl.Visibility == Visibility.Visible)
            {
                grid.RowDefinitions[0].Height = new GridLength(_currentSplitterPosition, GridUnitType.Star);
                grid.RowDefinitions[1].Height = new GridLength(30);
                grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                _currentSplitterPosition = reportControl.ActualHeight/taskboardControl.ActualHeight;

                grid.RowDefinitions[0].Height = new GridLength(0);
                grid.RowDefinitions[1].Height = new GridLength(0);
                grid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
            }
        }
    }
}
