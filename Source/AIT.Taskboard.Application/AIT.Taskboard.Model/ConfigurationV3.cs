using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AIT.Taskboard.Interface;
using System.Xml.Serialization;

namespace AIT.Taskboard.Model
{
    /// <summary>
    /// New version of Configuration class
    /// Has one new property: <see cref="HideColumnSummaryFieldName"/>
    /// To ensure backwards compatibility when adding new members, ConfigurationV4 must be made and 
    /// another try catch block added into <see cref="ReadTaskboardConfiguration"/>, member of 
    /// <see cref="ConfigurationService"/>
    /// </summary>
    [Serializable]
    public class ConfigurationV3: ConfigurationV2
    {
        const bool defaultHideColumnSummaryField = false;

        [XmlAnyElement]
        public bool HideColumnSummaryFieldName { get; set; }

        public ConfigurationV3()
            : base()
        {
            HideColumnSummaryFieldName = defaultHideColumnSummaryField;
        }

        public void CopyFromConfigurationOld2(ConfigurationV2 configOld2)
        {
            TeamProjectCollection = configOld2.TeamProjectCollection;
            TeamProject = configOld2.TeamProject;
            ChildItems.AddRange(configOld2.ChildItems);
            BacklogItems.AddRange(configOld2.BacklogItems);
            foreach (var state in configOld2.States)
            {
                States.Add(state.Clone() as ICustomState);
            }
            SortFieldName = configOld2.SortFieldName;
            SortDirection = configOld2.SortDirection;
            RowSummaryFieldName = configOld2.RowSummaryFieldName;
            ColumnSummaryFieldName = configOld2.ColumnSummaryFieldName;
            LinkType = configOld2.LinkType;
            QueryId = configOld2.QueryId;
            ReportId = configOld2.ReportId;
            ReportParameters.AddRange(configOld2.ReportParameters);
            IsAutoRefreshChecked = configOld2.IsAutoRefreshChecked;
            AutoRefreshDelay = configOld2.AutoRefreshDelay;
        }

        public object Clone()
        {
            ConfigurationV3 result = new ConfigurationV3();
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
            result.HideColumnSummaryFieldName = HideColumnSummaryFieldName;
            return result;
        }
    }
}
