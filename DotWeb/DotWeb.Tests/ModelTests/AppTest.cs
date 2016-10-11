using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotWeb.Tests.ModelTests
{
    [TestClass]
    public class AppTest
    {
        [TestMethod]
        public void App_initial_condition_test()
        {
            var app = new App();
            Assert.AreEqual(app.Groups.Count, 0);
        }

        [TestMethod]
        public void App_ToString_should_match_name_property()
        {
            var app = new App();
            app.Id = 1;
            app.Name = "Test App";
            app.Description = "Lorem ipsum dolor sit amet.";

            Assert.AreEqual(app.Name, app.ToString());
            Assert.AreEqual(app.GridTextColumnMaxLength, Constants.DefaultGridTextColumnMaxLength);
            Assert.AreEqual(app.PageSize, Constants.DefaultPageSize);
            Assert.AreEqual(app.DefaultGroupName, Constants.DefaultDefaultGroupName);
        }
    }
}
