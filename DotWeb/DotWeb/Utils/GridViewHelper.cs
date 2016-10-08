using DevExpress.Web;
using DotWeb.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace DotWeb.Utils
{
    public static class GridViewHelper
    {
        internal static GridViewCommandColumn AddGridViewCommandColumns()
        {
            var commandColumn = new GridViewCommandColumn();
            commandColumn.ShowEditButton = true;
            commandColumn.ShowDeleteButton = true;
            commandColumn.ShowNewButtonInHeader = true;

            return commandColumn;
        }

        internal static GridViewEditDataColumn AddGridViewDataColumn(ColumnMeta column, string connectionString)
        {
            GridViewEditDataColumn dataColumn = null;
            if ((new TypeCode[] { TypeCode.String, TypeCode.Int64, TypeCode.Int32, TypeCode.Single, TypeCode.Decimal })
                .Contains(column.DataType) && !column.IsForeignKey && string.IsNullOrEmpty(column.EnumTypeName))
            {
                var textColumn = new GridViewDataTextColumn();
                if (column.DataType == TypeCode.String && column.MaxLength.HasValue)
                    textColumn.PropertiesTextEdit.MaxLength = column.MaxLength.Value;
                dataColumn = textColumn;
            }
            else if (!string.IsNullOrEmpty(column.EnumTypeName))
            {
                var enumType = Type.GetType(column.EnumTypeName);
                if (enumType.IsEnum)
                {
                    var comboBoxColumn = new GridViewDataComboBoxColumn();
                    comboBoxColumn.PropertiesComboBox.ValueType = enumType;
                    comboBoxColumn.PropertiesComboBox.DataSource = Enum.GetValues(enumType);
                    dataColumn = comboBoxColumn;
                }

            }
            else if (column.IsForeignKey)
            {
                var comboBoxColumn = new GridViewDataComboBoxColumn();
                comboBoxColumn.PropertiesComboBox.DataSource = GetLookUpDataSource(column.ReferenceTable, connectionString);
                if (column.ReferenceTable.PrimaryKeys.Length > 1)
                    throw new ApplicationException(string.Format("Data source for lookup column {0} has more than one primary key.", column.Name));
                comboBoxColumn.PropertiesComboBox.ValueField = column.ReferenceTable.PrimaryKeys[0].Name;
                comboBoxColumn.PropertiesComboBox.TextField = column.ReferenceTable.LookUpDisplayColumn.Name;
                dataColumn = comboBoxColumn;
            }
            else if (column.DataType == TypeCode.DateTime)
                dataColumn = new GridViewDataDateColumn();
            else if (column.DataType == TypeCode.Boolean)
                dataColumn = new GridViewDataCheckColumn();
            else
                dataColumn = new GridViewDataTextColumn();

            if (column.IsPrimaryKey)
                dataColumn.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False;

            dataColumn.FieldName = column.Name;
            dataColumn.Caption = column.Caption;

            return dataColumn;
        }

        internal static SqlDataSource GetGridDataSource(TableMeta tableMeta, string connectionString, ColumnMeta foreignKey = null, 
            object value = null)
        {
            var ds = new SqlDataSource();
            ds.ConnectionString = connectionString;
            if (foreignKey == null)
            {
                ds.SelectCommand = SqlHelper.GenerateSelectQuery(tableMeta);
            }
            else
            {
                ds.SelectCommand = SqlHelper.GenerateSelectQueryDetail(tableMeta, foreignKey);
                ds.SelectParameters.Add(new Parameter(foreignKey.Name, foreignKey.DataType, value.ToString()));
            }
            ds.InsertCommand = SqlHelper.GenerateInsertQuery(tableMeta);
            ds.UpdateCommand = SqlHelper.GenerateUpdateQuery(tableMeta);
            ds.DeleteCommand = SqlHelper.GenerateDeleteQuery(tableMeta);

            return ds;
        }

        internal static SqlDataSource GetLookUpDataSource(TableMeta tableMeta, string connectionString)
        {
            var ds = new SqlDataSource();
            ds.ConnectionString = connectionString;
            ds.SelectCommand = SqlHelper.GenerateSelectQuery(tableMeta);

            return ds;
        }

    }
}
