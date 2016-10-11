using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotWeb.Tests.ModelTests
{
    [TestClass]
    public class ModuleTest
    {
        [TestMethod]
        public void Module_initial_condition_test()
        {
            var module = new Module();
            Assert.IsTrue(module.ShowInLeftMenu);
        }

        [TestMethod]
        public void Module_ToString_should_match_name_property()
        {
            var module = new Module();
            module.TableName = "MyTable";
            module.Title = "My Table";
            module.OrderNo = 1;

            Assert.AreEqual(module.Title, module.ToString());
        }
    }
}
