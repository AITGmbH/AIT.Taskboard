using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AIT.Taskboard.Application.Helper
{
    internal static class FieldHelper
    {
        private static IList<string> _fieldBlackList;

        internal static IList<string> FieldBlackList
        {
            get
            {
                // TODO: Refactor to retrieve black list from settings
                if (_fieldBlackList == null)
                {
                    _fieldBlackList = new List<string>
                                          {
                                              "Iteration ID", // TFS 2010 field name
                                              "IterationID", // TFS 2008 field name
                                              "Area ID", // TFS 2010 field
                                              "AreaID", // TFS 2008 field
                                              // Add VSTS Extension Fields
                                              "ParentWI",
                                              "FirstChildWI",
                                              "NextWI",
                                              // Add the kanban fields
                                              "Proposed Enter Completed Work",
                                              "Development Enter Completed Work",
                                              "Test Enter Completed Work",
                                              "Deployment Enter Completed Work",
                                              "Proposed Leave Completed Work",
                                              "Development Leave Completed Work",
                                              "Test Leave Completed Work",
                                              "Deployment Leave Completed Work"
                                          };
                }
                return _fieldBlackList;
            }
        }
    }
}
