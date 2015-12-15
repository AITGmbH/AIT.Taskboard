using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace AIT.Taskboard.Model
{
    public interface ITfsInformationProvider
    {
        Project TeamProject { get; set; }
        TfsTeamProjectCollection TeamProjectCollection { get; set; }
        void OnTfsCommunicationError(string message);
    }
}
