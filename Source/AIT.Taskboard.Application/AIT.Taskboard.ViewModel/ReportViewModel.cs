using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using AIT.Taskboard.Interface;
using System.ComponentModel;
using Microsoft.TeamFoundation.Client.Reporting;
using ReportParameter = Microsoft.Reporting.WinForms.ReportParameter;

namespace AIT.Taskboard.ViewModel
{
    public class ReportViewModel : INotifyPropertyChanged
    {
        private List<IHierarchicalItem> _reports;
        private IHierarchicalItem _currentReport;

        public ReportViewModel(ITaskboardService taskboardService, IConfigurationService configurationService, IConfiguration configuration)
        {
            TaskboardService = taskboardService;
            ConfigurationService = configurationService;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; private set; }

        public ITaskboardService TaskboardService { get; set; }

        private bool _hideReports;

        public bool HideReports
        {
            get { return _hideReports; }
            set { _hideReports = value; OnPropertyChanged("Reports"); }
        }

        public List<IHierarchicalItem> Reports
        {
            get
            {
                if (HideReports) return null;
                return _reports ?? (_reports = TaskboardService.GetReports());
            }
        }

        public IHierarchicalItem CurrentReport
        {
            get 
            {
                if (Configuration == null) return null;
                if (!String.IsNullOrEmpty(Configuration.ReportId))
                {
                    _currentReport = Reports.SelectMany(r => r.FlattenHierarchy(f => f.Children)).SingleOrDefault(r => r.Item.ID == Configuration.ReportId);
                }
                return _currentReport; 
            }
            set
            {
                _currentReport = value;
                if (value != null)
                {
                    if (Configuration == null) return;
                    Configuration.ReportId = value.Item.ID;
                    // TODO: Add the report parameters to the configuration as well.
                    Configuration.ReportParameters.Clear ();
                    Configuration.ReportParameters.AddRange(ReportParameters);
                    // TODO:Retrieve file name as configuration data
                    string configurationData = ConfigurationData;
                    //do not use the original Configuration object because it is reffered by the VieModel and cannot be serialized
                    ConfigurationService.SaveTaskboardConfiguration(configurationData, Configuration.Clone() as IConfiguration);
                }
                OnPropertyChanged("CurrentReport");
            }
        }

        protected IConfigurationService ConfigurationService { get; set; }

        public string ConfigurationData { get; set; }

        public List<Microsoft.Reporting.WinForms.ReportParameter> ReportParameters
        {
            get { return Configuration.ReportParameters; }
        }

        

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        public void SaveConfiguration()
        {
            if (Configuration == null) return;

            Configuration.ReportId = CurrentReport.Item.ID;
            
            // TODO:Retrieve file name as configuration data
            string configurationData = ConfigurationData;
            //do not use the original Configuration object because it is reffered by the VieModel and cannot be serialized
            ConfigurationService.SaveTaskboardConfiguration(configurationData, Configuration.Clone() as IConfiguration);
        }
    }
}
