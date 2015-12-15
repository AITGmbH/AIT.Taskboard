using System;
using System.Collections.Generic;
using AIT.Taskboard.Interface;
using AIT.Taskboard.Interface.Extensions;
using AIT.Taskboard.Model;
using Microsoft.Reporting.WinForms;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace AIT.Taskboard.UnitTests
{
    
    
    /// <summary>
    ///This is a test class for ConfigurationV2Test and is intended
    ///to contain all ConfigurationV2Test Unit Tests
    ///</summary>
    [TestClass()]
    public class ConfigurationV2Test
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for ConfigurationV2 Constructor
        ///</summary>
        [TestMethod()]
        public void ConfigurationV2ConstructorTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            // Tested in other methods 
        }

        private ConfigurationV2 CreateConfiguration()
        {
            ConfigurationV2 target = new ConfigurationV2();
            target.TeamProjectCollection = "TeamProjectCollection";
            target.TeamProject = "TeamProject";
            target.ChildItems.Add("ChildItems");
            target.BacklogItems.Add("BacklogItems");
            target.States.Add(new CustomState { Name = "States" });
            target.SortFieldName = "SortFieldName";
            target.SortDirection = SortDirection.Ascending;
            target.RowSummaryFieldName = "RowSummaryFieldName";
            target.ColumnSummaryFieldName = "ColumnSummaryFieldName";
            target.LinkType = "LinkType";
            target.QueryId = Guid.Empty;
            target.ReportId = "ReportId";
            target.ReportParameters.Add(new ReportParameter { Name = "Report Parameter" });
            target.IsAutoRefreshChecked = true;
            target.AutoRefreshDelay = 15;
            return target;
        }

        [TestMethod]
        public void TestSerializationDeserializationV2()
        {
            ConfigurationV2 target = new ConfigurationV2();
            target = CreateConfiguration();
            string serializedTarget = target.SerializeAsString();
            ConfigurationV2 copy = ConfigurationV2.DeserializeFromString(serializedTarget) as ConfigurationV2;

            Assert.AreEqual(copy.TeamProjectCollection, "TeamProjectCollection");
            Assert.AreEqual(copy.TeamProject, "TeamProject");
            Assert.AreEqual(copy.ChildItems[0], "ChildItems");
            Assert.AreEqual(copy.BacklogItems[0], "BacklogItems");
            Assert.AreEqual(copy.States[0].Name, "States");
            Assert.AreEqual(copy.SortFieldName, "SortFieldName");
            Assert.AreEqual(copy.SortDirection, SortDirection.Ascending);
            Assert.AreEqual(copy.RowSummaryFieldName, "RowSummaryFieldName");
            Assert.AreEqual(copy.ColumnSummaryFieldName, "ColumnSummaryFieldName");
            Assert.AreEqual(copy.LinkType, "LinkType");
            Assert.AreEqual(copy.QueryId, Guid.Empty);
            Assert.AreEqual(copy.ReportId, "ReportId");
            Assert.AreEqual(copy.ReportParameters[0].Name, "Report Parameter");
            Assert.AreEqual(copy.IsAutoRefreshChecked, true); 
            Assert.AreEqual(copy.AutoRefreshDelay, 15); 
        }

        /// <summary>
        ///A test for Clone
        ///</summary>
        [TestMethod()]
        public void CloneTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            target = CreateConfiguration();

            ConfigurationV2 clone = target.Clone() as ConfigurationV2;

            Assert.AreEqual(target.TeamProjectCollection, "TeamProjectCollection");
            Assert.AreEqual(target.TeamProject, "TeamProject");
            Assert.AreEqual(target.ChildItems[0],"ChildItems");
            Assert.AreEqual(target.BacklogItems[0], "BacklogItems");
            Assert.AreEqual(target.States[0].Name, "States");
            Assert.AreEqual(target.SortFieldName, "SortFieldName");
            Assert.AreEqual(target.SortDirection, SortDirection.Ascending);
            Assert.AreEqual(target.RowSummaryFieldName, "RowSummaryFieldName");
            Assert.AreEqual(target.ColumnSummaryFieldName, "ColumnSummaryFieldName");
            Assert.AreEqual(target.LinkType, "LinkType");
            Assert.AreEqual(target.QueryId, Guid.Empty);
            Assert.AreEqual(target.ReportId, "ReportId");
            Assert.AreEqual(target.ReportParameters[0].Name, "Report Parameter" );
            Assert.AreEqual(target.IsAutoRefreshChecked, true);
            Assert.AreEqual(target.AutoRefreshDelay, 15);            
        }

        /// <summary>
        ///A test for CopyFromConfigurationOld1
        ///</summary>
        [TestMethod()]
        public void CopyFromConfigurationOld1Test()
        {
            Configuration target = new Configuration();
            target.TeamProjectCollection = "TeamProjectCollection";
            target.TeamProject = "TeamProject";
            target.ChildItems.Add("ChildItems");
            target.BacklogItems.Add("BacklogItems");
            target.States.Add(new CustomState { Name = "States" });
            target.SortFieldName = "SortFieldName";
            target.SortDirection = SortDirection.Ascending;
            target.RowSummaryFieldName = "RowSummaryFieldName";
            target.ColumnSummaryFieldName = "ColumnSummaryFieldName";
            target.LinkType = "LinkType";
            target.QueryId = Guid.Empty;
            target.ReportId = "ReportId";
            target.ReportParameters.Add(new ReportParameter { Name = "Report Parameter" });

            ConfigurationV2 copy = new ConfigurationV2();
            copy.CopyFromConfigurationOld1(target);

            Assert.AreEqual(copy.TeamProjectCollection, "TeamProjectCollection");
            Assert.AreEqual(copy.TeamProject, "TeamProject");
            Assert.AreEqual(copy.ChildItems[0], "ChildItems");
            Assert.AreEqual(copy.BacklogItems[0], "BacklogItems");
            Assert.AreEqual(copy.States[0].Name, "States");
            Assert.AreEqual(copy.SortFieldName, "SortFieldName");
            Assert.AreEqual(copy.SortDirection, SortDirection.Ascending);
            Assert.AreEqual(copy.RowSummaryFieldName, "RowSummaryFieldName");
            Assert.AreEqual(copy.ColumnSummaryFieldName, "ColumnSummaryFieldName");
            Assert.AreEqual(copy.LinkType, "LinkType");
            Assert.AreEqual(copy.QueryId, Guid.Empty);
            Assert.AreEqual(copy.ReportId, "ReportId");
            Assert.AreEqual(copy.ReportParameters[0].Name, "Report Parameter");
            Assert.AreEqual(copy.IsAutoRefreshChecked, false); //default value
            Assert.AreEqual(copy.AutoRefreshDelay, 30); // default value
        }

        /// <summary>
        ///A test for AutoRefreshDelay
        ///</summary>
        [TestMethod()]
        public void AutoRefreshDelayTest()
        {
            ConfigurationV2 target = new ConfigurationV2(); 
            Assert.AreEqual(target.AutoRefreshDelay, 30); // 30 should be default value
            target.AutoRefreshDelay = 0;
            Assert.AreEqual(target.AutoRefreshDelay, 0);
            target.AutoRefreshDelay = 10;
            Assert.AreEqual(target.AutoRefreshDelay, 10);
        }

        /// <summary>
        ///A test for BacklogItems
        ///</summary>
        [TestMethod()]
        [DeploymentItem("AIT.Taskboard.Model.dll")]
        public void BacklogItemsTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.IsNotNull(target.BacklogItems);
            target.BacklogItems.Add("s1");
            target.BacklogItems.Add("s2");
            target.BacklogItems.Add("s3");
            Assert.AreEqual(target.BacklogItems.Count, 3);
            Assert.AreEqual(target.BacklogItems[0], "s1");
            Assert.AreEqual(target.BacklogItems[1], "s2");
            Assert.AreEqual(target.BacklogItems[2], "s3");
        }

        /// <summary>
        ///A test for ChildItems
        ///</summary>
        [TestMethod()]
        [DeploymentItem("AIT.Taskboard.Model.dll")]
        public void ChildItemsTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.IsNotNull(target.ChildItems);
            target.ChildItems.Add("s1");
            target.ChildItems.Add("s2");
            target.ChildItems.Add("s3");
            Assert.AreEqual(target.ChildItems.Count, 3);
            Assert.AreEqual(target.ChildItems[0], "s1");
            Assert.AreEqual(target.ChildItems[1], "s2");
            Assert.AreEqual(target.ChildItems[2], "s3");
        }

        /// <summary>
        ///A test for ColumnSummaryFieldName
        ///</summary>
        [TestMethod()]
        public void ColumnSummaryFieldNameTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.ColumnSummaryFieldName, default(string));
            target.ColumnSummaryFieldName = "column";
            Assert.AreEqual(target.ColumnSummaryFieldName, "column");
        }

        /// <summary>
        ///A test for InternalStates
        ///</summary>
        [TestMethod()]
        public void InternalStatesTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.IsNotNull(target.InternalStates);
            target.States.Add(new CustomState() { Name = "name 1" });
            target.States.Add(new CustomState() { Name = "name 2" });
            Assert.AreEqual(target.InternalStates.Count, 2);
            Assert.AreEqual(target.InternalStates[0].Name, "name 1");
            Assert.AreEqual(target.InternalStates[1].Name, "name 2");
            List<CustomState> list = target.InternalStates;
            target.InternalStates = new List<CustomState>();
            target.InternalStates = list;
            Assert.AreEqual(target.InternalStates.Count, 2);
            Assert.AreEqual(target.InternalStates[0].Name, "name 1");
            Assert.AreEqual(target.InternalStates[1].Name, "name 2");
        }

        /// <summary>
        ///A test for IsAssociatedWithTeamProject
        ///</summary>
        [TestMethod()]
        public void IsAssociatedWithTeamProjectTest()
        {
            ConfigurationV2 target = new ConfigurationV2(); 
            Assert.IsFalse(target.IsAssociatedWithTeamProject);
            target.TeamProject = null;
            target.TeamProjectCollection = null;
            Assert.IsFalse(target.IsAssociatedWithTeamProject);
            target.TeamProject = "some";
            target.TeamProjectCollection = null;
            Assert.IsFalse(target.IsAssociatedWithTeamProject);
            target.TeamProject = null;
            target.TeamProjectCollection = "thing";
            Assert.IsFalse(target.IsAssociatedWithTeamProject);
            target.TeamProject = "some";
            target.TeamProjectCollection = "thing";
            Assert.IsTrue(target.IsAssociatedWithTeamProject);
        }

        /// <summary>
        ///A test for IsAutoRefreshChecked
        ///</summary>
        [TestMethod()]
        public void IsAutoRefreshCheckedTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.IsAutoRefreshChecked, false); // false shlould be default value
            target.IsAutoRefreshChecked = false;
            Assert.AreEqual(target.IsAutoRefreshChecked, false);
            target.IsAutoRefreshChecked = true;
            Assert.AreEqual(target.IsAutoRefreshChecked, true);
        }

        /// <summary>
        ///A test for LinkType
        ///</summary>
        [TestMethod()]
        public void LinkTypeTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.LinkType, default(string));
            target.LinkType = "link type";
            Assert.AreEqual(target.LinkType, "link type");
        }

        /// <summary>
        ///A test for QueryId
        ///</summary>
        [TestMethod()]
        public void QueryIdTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.QueryId, default(Guid));
            target.QueryId = Guid.Empty;
            Assert.AreEqual(target.QueryId, Guid.Empty);
            Guid guid = new Guid();
            target.QueryId = guid;
            Assert.AreEqual(target.QueryId, guid);
        }

        /// <summary>
        ///A test for ReportId
        ///</summary>
        [TestMethod()]
        public void ReportIdTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.ReportId, default(string));
            target.ReportId = "reportID";
            Assert.AreEqual(target.ReportId, "reportID");
        }

        /// <summary>
        ///A test for ReportParameters
        ///</summary>
        [TestMethod()]
        public void ReportParametersTest()
        {
            ConfigurationV2 target = new ConfigurationV2(); 
            Assert.IsNotNull(target.ReportParameters);
            target.ReportParameters.Add(new ReportParameter() { Name = "report0" });
            target.ReportParameters.Add(new ReportParameter() { Name = "report1" });
            Assert.AreEqual(target.ReportParameters.Count, 2);
            Assert.AreEqual(target.ReportParameters[0].Name, "report0");
            Assert.AreEqual(target.ReportParameters[1].Name, "report1");
        }

        /// <summary>
        ///A test for RowSummaryFieldName
        ///</summary>
        [TestMethod()]
        public void RowSummaryFieldNameTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.RowSummaryFieldName, default(string));
            target.RowSummaryFieldName = "row";
            Assert.AreEqual(target.RowSummaryFieldName, "row");
        }

        /// <summary>
        ///A test for SortDirection
        ///</summary>
        [TestMethod()]
        public void SortDirectionTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.SortDirection, default(SortDirection));
            target.SortDirection = SortDirection.Ascending;
            Assert.AreEqual(target.SortDirection, SortDirection.Ascending);
            target.SortDirection = SortDirection.Descending;
            Assert.AreEqual(target.SortDirection, SortDirection.Descending);
        }

        /// <summary>
        ///A test for SortFieldName
        ///</summary>
        [TestMethod()]
        public void SortFieldNameTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.SortFieldName, default(string));
            target.SortFieldName = "sort field name";
            Assert.AreEqual(target.SortFieldName, "sort field name");
        }

        /// <summary>
        ///A test for States
        ///</summary>
        [TestMethod()]
        [DeploymentItem("AIT.Taskboard.Model.dll")]
        public void StatesTest()
        {
            ConfigurationV2 target = new ConfigurationV2(); 
            Assert.IsNotNull(target.States);
            target.States.Add(new CustomState() { Name = "name 1" });
            target.States.Add(new CustomState() { Name = "name 2" });
            Assert.AreEqual(target.States.Count, 2);
            Assert.AreEqual(target.States[0].Name, "name 1");
            Assert.AreEqual(target.States[1].Name, "name 2");
        }

        /// <summary>
        ///A test for TeamProject
        ///</summary>
        [TestMethod()]
        public void TeamProjectTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.TeamProject, default(string));
            target.TeamProject = "team project";
            Assert.AreEqual(target.TeamProject, "team project");
        }

        /// <summary>
        ///A test for TeamProjectCollection
        ///</summary>
        [TestMethod()]
        public void TeamProjectCollectionTest()
        {
            ConfigurationV2 target = new ConfigurationV2();
            Assert.AreEqual(target.TeamProject, default(string));
            target.TeamProject = "team project collection";
            Assert.AreEqual(target.TeamProject, "team project collection");
        }
    }
}
