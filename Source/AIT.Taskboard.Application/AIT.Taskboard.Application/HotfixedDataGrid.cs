using System.Windows.Automation.Peers;
using System.Windows.Controls;

namespace AIT.Taskboard.Application
{
    /// <summary>
    /// In the current state of development (August 2011) Mircosoft.Windows.Controls.DataGrid is bugged
    /// and has not been fixed yet (http://wpf.codeplex.com/workitem/14443). To prevent the exception 
    /// "Recursive call to Automation Peer API is not valid." to occur on some machines DataGrid has been 
    /// derived and hotfixed as described in http://stackoverflow.com/questions/4017786/wpf-recursive-call-to-automation-peer-api-is-not-valid.
    /// </summary>
    class HotfixedDataGrid : DataGrid
    {
        /// <summary>
        /// Hotfixed AutomationPeer.
        /// </summary>
        /// <returns></returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return null;
        }
    }
}
