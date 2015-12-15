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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AIT.Taskboard.ViewModel;

namespace AIT.Taskboard.Application.Controls
{
    /// <summary>
    /// Interaction logic for StatusView.xaml
    /// </summary>
    public partial class StatusView : UserControl
    {
        public StatusView()
        {
            InitializeComponent();
        }



        public StatusViewModel ViewModel
        {
            get { return (StatusViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(StatusViewModel), typeof(StatusView), new UIPropertyMetadata(null, HandleViewModelChanged));

        private static void HandleViewModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Just for debugging:
            System.Diagnostics.Trace.WriteLine(e.NewValue != null
                                                   ? "New Status View Model"
                                                   : "!!! No Status View Model !!!");
        }
    }
}
