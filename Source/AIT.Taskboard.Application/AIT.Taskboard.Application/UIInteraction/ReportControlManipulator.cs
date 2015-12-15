using System;
using Microsoft.Reporting.WinForms;

namespace AIT.Taskboard.Application.UIInteraction
{
    /// <summary>
    /// Manipulates the ReportControl visual representation.
    /// </summary>
    class ReportControlManipulator : IControlManipulation
    {
        private readonly ReportControl _owningControl;

        /// <summary>
        /// Creates a new instance of the ReportControlManipulator class.
        /// </summary>
        /// <param name="reportControl">ReportControl upon which all visual changes are performed.</param>
        public ReportControlManipulator(ReportControl reportControl)
        {
            if(reportControl == null) throw new ArgumentException("reportControl");
            _owningControl = reportControl;
        }

        #region IControlManipulation members

        public void ZoomControl(double delta)
        {
            if (delta == 0 || delta == 1) return;

            ReportViewer rv = _owningControl.viewerInstance;
            rv.ZoomMode = ZoomMode.Percent;
            int currZoomValue = rv.ZoomPercent;
            double newZoomValue = currZoomValue * delta;
            rv.ZoomPercent = (int)newZoomValue;
        }

        #endregion
    }
}
