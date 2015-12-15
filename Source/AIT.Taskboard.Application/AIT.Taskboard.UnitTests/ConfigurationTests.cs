using System;
using System.Collections.Generic;
using System.Windows.Media;
using AIT.Taskboard.Interface;
using AIT.Taskboard.Interface.Extensions;
using AIT.Taskboard.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AIT.Taskboard.UnitTests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {get;set;}

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void TestSerializationDeserialization()
        {
            ConfigurationV5 config = CreateConfiguration();
            string serializedConfig = config.SerializeAsString();
            var result = serializedConfig.DeserializeFromString();
            Assert.AreEqual(config.ColumnSummaryFieldName, result.ColumnSummaryFieldName);
            Assert.AreEqual(config.RowSummaryFieldName, result.RowSummaryFieldName);
            Assert.AreEqual(config.SortFieldName, result.SortFieldName);
            Assert.AreEqual(config.SortDirection, result.SortDirection);
            Assert.AreEqual(config.QueryId, result.QueryId);
            Assert.AreEqual(config.States.Count, result.States.Count);
            Assert.AreEqual(config.BacklogItems.Count, result.BacklogItems.Count);
            Assert.AreEqual(config.ChildItems.Count, result.ChildItems.Count);
            Assert.AreEqual(config.HideColumnSummaryFieldName, result.HideColumnSummaryFieldName);
        }

        protected virtual ConfigurationV5 CreateConfiguration()
        {
            var config = new ConfigurationV5();
            config.BacklogItems.AddRange(new List<string> {"Requirement", "Bug"});
            config.ChildItems.AddRange(new List<string> {"Task", "TestCase"});
            config.ColumnSummaryFieldName = "Remaining Work";
            config.RowSummaryFieldName = "Completed Work";
            config.QueryId = Guid.NewGuid();
            config.SortDirection = SortDirection.Descending;
            config.SortFieldName = "Priority";
            config.HideColumnSummaryFieldName = true;
            config.WorkItemSize = WorkItemSize.Small;
            var stateA = new CustomState {Color = Colors.AliceBlue, Name = "Pending"};
            stateA.WorkItemStates.Add("Proposed");
            var stateB = new CustomState { Color = Colors.Navy, Name = "In Progress" };
            stateB.WorkItemStates.Add("Active");
            var stateC = new CustomState { Color = Colors.DarkRed, Name = "Done" };
            stateC.WorkItemStates.Add("Resolved");
            stateC.WorkItemStates.Add("Closed");

            config.States.Add(stateA);
            config.States.Add(stateB);
            config.States.Add(stateC);
            return config;
        }
    }
}
