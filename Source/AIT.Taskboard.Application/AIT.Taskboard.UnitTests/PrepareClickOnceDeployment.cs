using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AIT.Taskboard.UnitTests
{
    [TestClass]
    public class PrepareClickOnceDeployment
    {
        #region Public Functions

        /// <summary>
        /// Prepares the content includes for click once deployment.
        /// </summary>
        [TestMethod]
        public void PrepareContentIncludesForClickOnceDeployment()
        {
            var clickOncePreparer = new ClickOnceDeploymentPreparer("AIT.Taskboard.Application.csproj");

            // TODO: Existing References need to get excluded. Implement a more generic way.
            var excludes = new List<string>
                               {
                                   @"^.+\.((pdb)|(xml)|(exe)|(exe.config)|(lic)|(config))$",
                                   @"^.*System.+\.dll$",
                                   @"^.*RibbonControlsLibrary.dll$",
                                   @"^.*WPFToolkit.*\.dll$",
                                   @"^.*AIT.Taskboard.Interface.dll$",
                                   @"^.*AIT.Taskboard.ViewModel.dll$",
                                   @"^.*FluidKit.dll$",
                                   @"^.*DevComponents.WpfEditors.dll$",
                                   @"^.*Microsoft.TeamFoundation.+\.dll$",
                                   @"^.*Microsoft.TeamFoundation.dll$"
                               };

            clickOncePreparer.AddContentItemsToClickOnceApplication(excludes);
        }

        #endregion
    }
}