using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotWeb.Tests.ModelTests
{
    [TestClass]
    public class TableMetaTest
    {
        [TestMethod]
        public void TableMeta_initial_condition_test()
        {
            var tableMeta = new TableMeta();
            Assert.AreEqual(tableMeta.Columns.Count, 0);
            Assert.AreEqual(tableMeta.Children.Count, 0);
            Assert.AreEqual(tableMeta.Parents.Count, 0);
            try
            {
                // At this point tableMeta does not have primary keys
                var primaryKeys = tableMeta.PrimaryKeys;
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentException));
            }
        }

        [TestMethod]
        public void TableMeta_ToString_should_match_name_property()
        {
            var tableMeta = new TableMeta();
            tableMeta.Name = "MyTable";
            tableMeta.SchemaName = "dbo";
            tableMeta.Caption = "My Table";

            Assert.AreEqual(tableMeta.Name, tableMeta.ToString());
        }

        [TestMethod]
        public void TableMeta_primary_keys_defined()
        {
            var tableMeta = new TableMeta();
            tableMeta.Name = "MyTable";
            tableMeta.SchemaName = "dbo";
            tableMeta.Caption = "My Table";
            tableMeta.Columns.Add(new ColumnMeta() 
                { Name = "Id", OrderNo = 1, DataType = TypeCode.Int32, IsPrimaryKey = true, IsRequired = true, Table = tableMeta }
            );

            Assert.IsTrue(tableMeta.PrimaryKeys.Length == 1);
        }

        [TestMethod]
        public void TableMeta_composite_primary_keys_defined()
        {
            var tableMeta = new TableMeta();
            tableMeta.Name = "MyTable";
            tableMeta.SchemaName = "dbo";
            tableMeta.Caption = "My Table";
            tableMeta.Columns.Add(new ColumnMeta() 
                { Name = "OrderId", OrderNo = 1, DataType = TypeCode.Int32, IsPrimaryKey = true, IsRequired = true, Table = tableMeta }
            );
            tableMeta.Columns.Add(new ColumnMeta() 
                { Name = "CustomerId", OrderNo = 2, DataType = TypeCode.Int32, IsPrimaryKey = true, IsRequired = true, Table = tableMeta }
            );

            Assert.IsTrue(tableMeta.PrimaryKeys.Length == 2);
        }
    }
}
