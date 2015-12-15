using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Controls;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Interface
{
    public interface ITaskboardService
    {
        WorkItemCollection WorkItems { get; }
        WorkItemLinkInfo[] WorkItemLinkInfos { get; }
        List<WorkItem> BackLogItems { get; }
        List<WorkItem> AllChildren { get; }
        ObservableCollection<IBacklogChildren> BacklogChildren { get; }
        ILoginData LoginData { get; set; }
        string ReportFolder { get; }
        string ReportServerUrl { get; }
        bool IsConnected { get; }
        void Connect();
        void ApplyConfiguration(IConfiguration configuration);
        WorkItemTypeCollection GetWorkItemTypes();
        QueryHierarchy GetWorkItemQueries(bool refreshQueryHierarchy);
        ICollection<Node> GetIterationPaths();
        ICollection<Node> GetAreaPaths();
        List<FieldDefinition> GetPossibleSummaryFields(QueryItem queryItem);
        List<FieldDefinition> GetPossibleSortFields(QueryItem queryItem);
        WorkItemLinkTypeCollection GetLinkTypes();
        string GetTargetState(WorkItem workItem, ICustomState targetState);
        QueryItem GetQueryItem(Guid queryId);
        List<IHierarchicalItem> GetReports();
        event EventHandler<ErrorEventArgs> ConnectionFailed;
        event EventHandler<ErrorEventArgs> TfsCommunicationError;
        event EventHandler<LoginDataEventArgs> RequestLoginData;
        event EventHandler<WorkItemIssueEventArgs> WorkItemIssuesOccurred;
        event EventHandler<RequestEventArgs> RequestSaveWorkItems;
        event EventHandler Connected;
        event EventHandler Disconnected;
        event EventHandler ConfigurationApplied;
        void SaveChanges();
        void Disconnect();
        IBacklogChildren GetBacklogFromWorkItem(WorkItem workItem);
        bool HandleConnectionException(Exception ex);
        WorkItem CreateNewLinkedWorkItem(WorkItem parent,
                                            WorkItemLinkTypeEnd linkType,
                                            WorkItemType workItemType,
                                            string title,
                                            string comment);
        void ApplyConfigurationToLink(WorkItem item, WorkItem parent);
        void SaveLinkedItem(WorkItem parent, WorkItem item, WorkItemLinkTypeEnd linkType);
    }
}