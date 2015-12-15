using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AIT.Taskboard.Interface;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AIT.Taskboard.Model
{
    /// <summary>
    /// New version of Configuration class
    /// Has two new properties: <see cref="AutoRefreshChecked"/> and <see cref="AutoRefreshDelay"/>
    /// To ensure backwards compatibility when adding new members, ConfigurationV3 must be made and 
    /// another try catch block added into <see cref="ReadTaskboardConfiguration"/>, member of 
    /// <see cref="ConfigurationService"/>
    /// </summary>
    [Serializable]
    public class ConfigurationV2
    {
        // defult status of AutoRefreshButton
        const bool defaultIsAutoRefreshChecked = false;
        // default time interval between autoRefreshes. In minutes
        const int defaultAutoRefreshDelay = 30;

        public ConfigurationV2()
        {
            ChildItems = new List<string>();
            BacklogItems = new List<string>();
            States = new List<ICustomState>();
            ReportParameters = new List<ReportParameter>();

            // sets default values for optional properties
            IsAutoRefreshChecked = defaultIsAutoRefreshChecked;
            AutoRefreshDelay = defaultAutoRefreshDelay;
        }

        public string TeamProjectCollection { get; set; }
        public string TeamProject { get; set; }
        [XmlArray]
        public List<string> ChildItems { get; private set; }
        [XmlArray]
        public List<string> BacklogItems { get; private set; }
        [XmlIgnore]
        public List<ICustomState> States { get; private set; }
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
        /// IsChecked property of AutoRefresh ToggleBox.
        /// </summary>
        [XmlAnyElement]
        public bool IsAutoRefreshChecked { get; set; }

        /// <summary>
        /// Delay between auto refreshes. Determined in seconds.
        /// </summary>
        [XmlAnyElement]
        public int AutoRefreshDelay { get; set; }


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

        /// <summary>
        /// Used to convert member of old Configuration class to ConfigurationV2 
        /// </summary>
        /// <param name="configOld1">old member of Configuration</param>
        public void CopyFromConfigurationOld1(Configuration configOld1)
        {
            TeamProjectCollection = configOld1.TeamProjectCollection;
            TeamProject = configOld1.TeamProject;
            ChildItems.AddRange(configOld1.ChildItems);
            BacklogItems.AddRange(configOld1.BacklogItems);
            foreach (var state in configOld1.States)
            {
                States.Add(state.Clone() as ICustomState);
            }
            SortFieldName = configOld1.SortFieldName;
            SortDirection = configOld1.SortDirection;
            RowSummaryFieldName = configOld1.RowSummaryFieldName;
            ColumnSummaryFieldName = configOld1.ColumnSummaryFieldName;
            LinkType = configOld1.LinkType;
            QueryId = configOld1.QueryId;
            ReportId = configOld1.ReportId;
            ReportParameters.AddRange(configOld1.ReportParameters);
            IsAutoRefreshChecked = defaultIsAutoRefreshChecked;
            AutoRefreshDelay = defaultAutoRefreshDelay;
        }

        public object Clone()
        {
            ConfigurationV2 result = new ConfigurationV2();
            result.TeamProjectCollection = TeamProjectCollection;
            result.TeamProject = TeamProject;
            result.ChildItems.AddRange(ChildItems);
            result.BacklogItems.AddRange(BacklogItems);
            foreach (var state in States)
            {
                result.States.Add(state.Clone() as ICustomState);
            }
            result.SortFieldName = SortFieldName;
            result.SortDirection = SortDirection;
            result.RowSummaryFieldName = RowSummaryFieldName;
            result.ColumnSummaryFieldName = ColumnSummaryFieldName;
            result.LinkType = LinkType;
            result.QueryId = QueryId;
            result.ReportId = ReportId;
            result.ReportParameters.AddRange(ReportParameters);
            result.IsAutoRefreshChecked = IsAutoRefreshChecked;
            result.AutoRefreshDelay = AutoRefreshDelay;
            return result;
        }

        public string SerializeAsString()
        {
            var formatter = new BinaryFormatter();
            using (var serializationStream = new MemoryStream())
            {
                formatter.Serialize(serializationStream, this);
                serializationStream.Seek(0, SeekOrigin.Begin);
                using (var reader = new BinaryReader(serializationStream))
                {
                    return Convert.ToBase64String(reader.ReadBytes((int)serializationStream.Length));
                }
            }
        }

        public static ConfigurationV2 DeserializeFromString(string serializedValue)
        {
            if (string.IsNullOrEmpty(serializedValue)) return new ConfigurationV2();
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(Convert.FromBase64String(serializedValue));
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    var formatter = new BinaryFormatter();
                    try
                    {
                        return formatter.Deserialize(memoryStream) as ConfigurationV2;
                    }
                    catch
                    {
                        return new ConfigurationV2();
                    }
                }
            }
        }
    }
}
