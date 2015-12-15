using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using AIT.Taskboard.Application.UIInteraction;
using AIT.Taskboard.ViewModel;
using Microsoft.Reporting.WinForms;
using System.Windows;
using AIT.Taskboard.Interface;
using System.Linq;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// Interaction logic for ReportControl.xaml
    /// </summary>
    public partial class ReportControl : UserControl
    {
        private ReportViewModel _model;
        
        public ReportControl()
        {
            InitializeComponent();
            InitMultiTouchSupport();
        }

        ReportViewModel Model
        {
            get { return _model; }
            set 
            { 
                _model = value;
            }
        }

        #region Multitouch Support

        private GestureEventManager _gestureEventManager;
        private IControlManipulation _manipulator;

        private void InitMultiTouchSupport()
        {
            _manipulator = new ReportControlManipulator(this);
            _gestureEventManager = new GestureEventManager(_manipulator);
        }

        #region GestureSupport especially for ReportControl

        private GestureController _gc;
        
        /// <summary>
        /// Init Support to process WM_Gesture Messages.
        /// 
        /// Has to be run AFTER everything else has been initialized.
        /// </summary>
        private void InitGestureSupport()
        {
            _gc = new GestureController();

            _gc.GestureBegin += OnGestureBegin;
            _gc.GestureEnd += OnGestureEnd;
            _gc.GesturePan += OnGesturePan;
            _gc.GestureZoom += OnGestureZoom;

            //Debug.Print("Gesture Support added");
        }

        /// <summary>
        /// Process WM_GESTURE_ZOOM
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGestureZoom(object sender, GestureEventArgs e)
        {
            _gestureEventManager.ProcessGestureZoom(e);
        }

        /// <summary>
        /// Process WM_GESTURE_PAN
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGesturePan(object sender, GestureEventArgs e)
        {
            _gestureEventManager.ProcessGesturePan(e);
        }

        /// <summary>
        /// Process WM_GESTURE_END
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGestureEnd(object sender, GestureEventArgs e)
        {
            _gestureEventManager.ProcessGestureEnd(e);
        }

       /// <summary>
        /// Process WM_GESTURE_BEGIN
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void OnGestureBegin(object sender, GestureEventArgs e)
        {
            _gestureEventManager.ProcessGestureBegin(e);
        }

        #endregion

        #endregion

        private void HandleReportSelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (reportItems.IsDropDownOpen)
            {
                var viewModel = DataContext as ReportViewModel;
                if (viewModel != null)
                {
                    viewModel.CurrentReport = e.NewValue as IHierarchicalItem;
                }
                reportItems.IsDropDownOpen = false;
            }
            LoadReport();
        }

        private void LoadReport()
        {
            if (Model != null &&
                Model.TaskboardService != null &&
                Model.CurrentReport != null &&
                !string.IsNullOrEmpty(Model.TaskboardService.ReportServerUrl) &&
                !string.IsNullOrEmpty(Model.TaskboardService.ReportFolder))
            {
                var reportServerUrl = Model.TaskboardService.ReportServerUrl;
                var cutOffIndex = reportServerUrl.LastIndexOf("/");
                reportServerUrl = reportServerUrl.Substring(0, cutOffIndex);
                reportServerUrl = reportServerUrl.ToLowerInvariant();

                var reportPath = Model.CurrentReport.Item.Path;
               
                // Set the processing mode for the ReportViewer to Remote
                viewerInstance.ProcessingMode = ProcessingMode.Remote;
                ServerReport serverReport = viewerInstance.ServerReport;
                // Get a reference to the report server credentials
                ReportServerCredentials rsCredentials = serverReport.ReportServerCredentials;
                // Set the credentials for the server report
                rsCredentials.NetworkCredentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                // Set the report server URL and report path
                serverReport.ReportServerUrl = new Uri(reportServerUrl);
                serverReport.ReportPath = reportPath;
                var reportParameters = GetReportParameters();
                if (reportParameters != null && reportParameters.Count > 0)
                    serverReport.SetParameters(reportParameters);
                // Refresh the report
                viewerInstance.RefreshReport();

            }
            else
            {
                viewerInstance.Clear();
            }
        }

        private IList<ReportParameter> GetReportParameters()
        {
            // TODO: Implement reading paramters from the configuration file
            return Model.ReportParameters;
        }
       
        private void SaveReportParameters()
        {
            var parameters = viewerInstance.ServerReport.GetParameters();
            if (parameters != null)
            {
                Model.ReportParameters.Clear();
                foreach (ReportParameterInfo reportParameterInfo in parameters)
                {
                    // Parameters that have no prompt are obviuosly not shown to the user
                    // We use this to filter out the "Project" parameter
                    if (reportParameterInfo.PromptUser && !string.IsNullOrEmpty(reportParameterInfo.Prompt))
                    {
                        System.Diagnostics.Trace.WriteLine(string.Format("{0}: {1}", reportParameterInfo.Name, reportParameterInfo.Values));
                        Model.ReportParameters.Add(new ReportParameter(reportParameterInfo.Name, reportParameterInfo.Values.ToArray()));
                    }
                }
                Model.SaveConfiguration();
            }
        }

        /// <summary>
        /// Loading control is completed, Set Model and Init Gesture Support
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Model = DataContext as ReportViewModel;

            //Init Gesture Support after loading the control is completed
            InitGestureSupport();
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Model = e.NewValue as ReportViewModel;

            if (Model != null && Model.Configuration != null && !Model.Configuration.HideReportViewer)
                LoadReport();
        }

        private void HandleReportViewerViewButtonClicked(object sender, CancelEventArgs e)
        {
            SaveReportParameters();
        }

        private void UserControlIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if (Model == null || !IsVisible) return;

            ////reportItems.ItemsSource = Model.Reports;
            ////reportItems.Items.Refresh();
        }
    }
}
