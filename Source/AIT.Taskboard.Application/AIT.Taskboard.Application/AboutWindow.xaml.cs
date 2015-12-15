using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using AIT.Taskboard.ViewModel;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when the user clicks the button 'buttonOK'.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains an event data.</param>
        private void HandleButtonOKClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Called when navigation events are requested.
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private void HandleHyperlinkWebRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string absoluteUri = e.Uri.AbsoluteUri;
            Process.Start(new ProcessStartInfo(absoluteUri));
            e.Handled = true;
        }

        private void HandleHyperlinkLicenseRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string absoluteUri = e.Uri.AbsoluteUri;
            Process.Start(new ProcessStartInfo(absoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// Called when navigation events are requested.
        /// </summary>
        /// <param name="sender">The object where the event handler is attached.</param>
        /// <param name="e">The event data.</param>
        private void HandleHyperlinkNavigate(object sender, RequestNavigateEventArgs e)
        {
            string absoluteUri = e.Uri.AbsoluteUri;
            Process.Start(new ProcessStartInfo(absoluteUri));
            e.Handled = true;
        }

        /// <summary>
        /// Called when the user clicks the button 'Register'.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains an event data.</param>
        private void ButtonRegister_Click(object sender, RoutedEventArgs e)
        {
            AboutModel aboutModel = (AboutModel)this.FindResource("model");
        }

        /// <summary>
        /// Called when the user clicks the button 'Terminate license'.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">A <see cref="RoutedEventArgs"/> that contains an event data.</param>
        private void ButttonTerminate_Click(object sender, RoutedEventArgs e)
        {
            AboutModel aboutModel = (AboutModel)this.FindResource("model");
        }

        [Import]
        public AboutModel Model
        {
            get { return DataContext as AboutModel; }
            set { DataContext = value; }
        }
    }
}
