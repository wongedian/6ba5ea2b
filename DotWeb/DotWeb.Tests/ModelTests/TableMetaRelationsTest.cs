using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotWeb.Tests.ModelTests
{
    [TestClass]
    public class TableMetaRelationsTest
    {
        [TestMethod]
        public void TableMetaRelations_initial_condition_test()
        {
            var tableMetaRelation = new TableMetaRelation();
            Assert.IsTrue(tableMetaRelation.IsRendered);
        }

        [TestMethod]
        public void TableMetaRelations_ToString_should_match_child()
        {
            var tableMetaRelation = new TableMetaRelation();
            tableMetaRelation.Child = new TableMeta() { Name = "MyTable" };

            Assert.AreEqual(tableMetaRelation.ToString(), tableMetaRelation.Child.ToString());
        }
    }
}
