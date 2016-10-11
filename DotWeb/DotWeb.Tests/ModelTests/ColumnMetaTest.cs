using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotWeb.Tests.ModelTests
{
    [TestClass]
    public class ColumnMetaTest
    {
        [TestMethod]
        public void ColumnMeta_initial_condition_test()
        {
            var columnMeta = new ColumnMeta();
            Assert.AreEqual(columnMeta.DataType, TypeCode.Empty);
            Assert.IsFalse(columnMeta.IsForeignKey);
            Assert.IsFalse(columnMeta.IsPrimaryKey);
            Assert.IsTrue(columnMeta.DisplayInGrid);
        }

        [TestMethod]
        public void ColumnMeta_ToString_should_match_name_datatype_property()
        {
            var columnMeta = new ColumnMeta();
            columnMeta.Name = "Id";
            columnMeta.DataType = TypeCode.Int32;
            Assert.AreEqual(columnMeta.ToString(), "Id: Int32");
        }
    }
}
