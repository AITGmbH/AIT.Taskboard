using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace AIT.Taskboard.Application.Helper
{
    public class RowSummaryItem
    {
        public RowSummaryItem(Brush brush, double width)
        {
            Brush = brush;
            Width = width;
            OpacityWidth = 1;
        }

        public Brush Brush { get; set; }
        public double Width { get; set; }
        public double OpacityWidth { get; set; }
    }
}
