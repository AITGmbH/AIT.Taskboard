using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AIT.Taskboard.Interface;
using Microsoft.Reporting.WinForms;

namespace AIT.Taskboard.Model
{
    /// <summary>
    /// Old version of Configuration class. New is ConfigurationV2
    /// This class must remain to ensure backwards compatibility,
    /// </summary>
    [Serializable]
    public class Configuration
    {
        public Configuration()
        {
            ChildItems = new List<string>();
            BacklogItems = new List<string>();
            States = new List<ICustomState>();
            ReportParameters = new List<ReportParameter>();
        }

        public string TeamProjectCollection { get; set; }
        public string TeamProject { get; set; }
        [XmlArray]
        public List<string> ChildItems { get; private set; }
        [XmlArray]
        public List<string> BacklogItems { get; private set; }
        [XmlIgnore]
        public List<ICustomState> States { get;private set; }
        public string SortFieldName { get; set; }
        public SortDirection SortDirection { get; set; }
        public string RowSummaryFieldName { get; set; }
        public string ColumnSummaryFieldName { get; set; }
        public string LinkType { get; set; }
        public Guid QueryId { get; set; }
        public string ReportId { get; set; }
        [OptionalField]
        private List<ReportParameter> _reportParameters;
        public List<ReportParameter> ReportParameters
        {
            get
            {
                if (_reportParameters == null)
                    _reportParameters = new List<ReportParameter>();
                return _reportParameters;
            }
            set { _reportParameters = value; }
        }

        public bool IsAssociatedWithTeamProject
        {
            get
            {
                // A configuration is considered to be associated with a team project if the TPC and team project are set
                return !string.IsNullOrEmpty(TeamProject) && !string.IsNullOrEmpty(TeamProjectCollection);
            }
        }

        /// <summary>
        /// This property is only intended to be used for de/serialization
        /// </summary>
        public List<CustomState> InternalStates
        {
            get { return new List<CustomState>(States.ConvertAll(state => state as CustomState)); }
            set
            {
                States.Clear();
                States.AddRange(value);
            }
        }
    }
}
