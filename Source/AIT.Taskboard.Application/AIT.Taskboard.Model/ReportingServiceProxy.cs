using System;
using System.Reflection;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Client.Reporting;

namespace AIT.Taskboard.Model
{
    internal class ReportingServiceProxy
    {
        #region Fields

        private const string ReportService = "ReportService.asmx";
        private const string ReportService2005 = "ReportService2005.asmx";

        private static readonly Assembly TeamFoundationClientAssembly = typeof(TfsTeamProjectCollection).Assembly;
        private static readonly Type ReportingServiceType = TeamFoundationClientAssembly.GetType("Microsoft.TeamFoundation.Client.Reporting.ReportingService");
        private static readonly MethodInfo ListChildrenMethod = ReportingServiceType.GetMethod("ListChildren", BindingFlags.Instance | BindingFlags.Public);

        private readonly TfsTeamProjectCollection _tpc;
        private TeamFoundationSoapProxy _proxy;
        private string _reportServerUrl;

        #endregion

        #region Properties

        public TfsTeamProjectCollection TeamProjectCollection
        {
            get { return _tpc; }
        }

        public string ReportServiceUrl
        {
            get
            {
                return this._proxy != null ? this._proxy.Url : string.Empty;
            }
        }

        public string ReportServerUrl
        {
            get
            {
                if ((this._reportServerUrl == null) && (this._proxy != null))
                {
                    this._reportServerUrl = this._proxy.Url.Replace(ReportService, string.Empty);
                    this._reportServerUrl = this._proxy.Url.Replace(ReportService2005, string.Empty);
                    this._reportServerUrl = this._reportServerUrl.TrimEnd('/');
                }

                return this._reportServerUrl;
            }
        }

        #endregion

        #region Constructors

        public ReportingServiceProxy(TfsTeamProjectCollection tpc)
        {
            if (tpc == null)
                throw new ArgumentNullException("tpc");

            _tpc = tpc;

            object reportingService = null;

            try
            {
                reportingService = tpc.GetService(ReportingServiceType);
            }
            catch (NullReferenceException)
            {
                //TODO: DragosH 2/23/2012 This exception is thrown when TFS used to connect to does not have reporting services features installed. Log this error message.
            }

            if (reportingService != null)
                _proxy = reportingService as TeamFoundationSoapProxy;
        }

        #endregion

        #region Methods

        public CatalogItem[] ListChildren(string item, bool recursive)
        {
            if (this._proxy == null)
            {
                return new CatalogItem[0];
            }

            return ListChildrenMethod.Invoke(this._proxy, new object[] {item, recursive}) as CatalogItem[];
        }

        #endregion
    }

}
