using DevExpress.Web;
using DotWeb.Utils;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    /// <summary>
    /// Helper methods for <see cref="ASPxGridView"/>.
    /// </summary>
    public static class GridViewHelper
    {
        /// <summary>
        /// Create <see cref="GridViewCommandColumn"/> instance containing Edit and Delete buttons. The New button
        /// is located on the header only, not on every row.
        /// </summary>
        /// <returns>An instance of <see cref="GridViewCommandColumn"/></returns>
        internal static GridViewCommandColumn AddGridViewCommandColumns()
        {
            var commandColumn = new GridViewCommandColumn();
            commandColumn.ShowEditButton = true;
            commandColumn.ShowDeleteButton = true;
            commandColumn.ShowNewButtonInHeader = true;

            return commandColumn;
        }

        /// <summary>
        /// Returns <see cref="GridViewEditDataColumn"/> instance based on meta data passed in column argument.
        /// </summary>
        /// <param name="column">An instance of <see cref="ColumnMeta"/> containing meta data of column to be created.</param>
        /// <param name="connectionString">Connection string to underlying database.</param>
        /// <returns></returns>
        internal static GridViewEditDataColumn AddGridViewDataColumn(ColumnMeta column, string connectionString)
        {
            GridViewEditDataColumn dataColumn = null;
            
            // As per 11 Oct 2016, the following evaluation criteria for data type and its column editor are not yet complete.
            // We still has some works to do to deal with picture and blob/document. (--github.com/ganagus)
            // ---
            if ((new TypeCode[] { TypeCode.String, TypeCode.Int64, TypeCode.Int32, TypeCode.Single, TypeCode.Decimal })
                .Contains(column.DataType) && !column.IsForeignKey && string.IsNullOrEmpty(column.EnumTypeName))
            {
                // regular text column, editor => text box
                dataColumn = AddGridViewDataTextColumn(column, dataColumn);
            }
            else if (!string.IsNullOrEmpty(column.EnumTypeName))
            {
                // enum data type column, editor => check box
                dataColumn = AddGridViewEnumColumn(column, dataColumn);
            }
            // foreign key should be rendered in combo box column
            else if (column.IsForeignKey)
            {
                // foreign key column, it's a look up to other table, editor => combo box
                dataColumn = AddGridViewForeignKeyColumn(column, dataColumn, connectionString);
            }
            else if (column.DataType == TypeCode.DateTime)
            {
                // date / datetime column, editor => DevExpress date editor
                dataColumn = new GridViewDataDateColumn();
            }
            else if (column.DataType == TypeCode.Boolean)
            {
                // boolean column, editor => check box
                dataColumn = new GridViewDataCheckColumn();
            }
            else
            {
                // other data type, default to text box
                dataColumn = new GridViewDataTextColumn();
            }

            if (column.IsPrimaryKey && !column.IsForeignKey)
                dataColumn.EditFormSettings.Visible = DevExpress.Utils.DefaultBoolean.False;

            dataColumn.FieldName = column.Name;
            dataColumn.Caption = column.Caption;
            dataColumn.VisibleIndex = column.OrderNo;

            return dataColumn;
        }

        /// <summary>
        /// Renders a regular text box grid view column.
        /// </summary>
        /// <param name="column">Meta data about column, an instance of <see cref="ColumnMeta"/>.</param>
        /// <param name="dataColumn">An instance of <see cref="GridViewEditDataColumn"/>.</param>
        /// <returns>An instance of <see cref="GridViewDataTextColumn"/>.</returns>
        private static GridViewEditDataColumn AddGridViewDataTextColumn(ColumnMeta column, GridViewEditDataColumn dataColumn)
        {
            var textColumn = new GridViewDataTextColumn();
            if (column.DataType == TypeCode.String && column.MaxLength.HasValue)
                textColumn.PropertiesTextEdit.MaxLength = column.MaxLength.Value;
            dataColumn = textColumn;
            return dataColumn;
        }

        /// <summary>
        /// Renders a combo box grid view column representing enumeration values.
        /// TODO: this code still does not work, need more effort. (github.com/ganagus).
        /// </summary>
        /// <param name="column">Meta data about column, an instance of <see cref="ColumnMeta"/>.</param>
        /// <param name="dataColumn">An instance of <see cref="GridViewEditDataColumn"/>.</param>
        /// <returns>An instance of <see cref="GridViewDataComboBoxColumn"/>.</returns>
        private static GridViewEditDataColumn AddGridViewEnumColumn(ColumnMeta column, GridViewEditDataColumn dataColumn)
        {
            var enumType = Type.GetType(column.EnumTypeName);
            if (enumType.IsEnum)
            {
                var comboBoxColumn = new GridViewDataComboBoxColumn();
                comboBoxColumn.PropertiesComboBox.ValueType = enumType;
                comboBoxColumn.PropertiesComboBox.DataSource = Enum.GetValues(enumType);
                dataColumn = comboBoxColumn;
            }
            return dataColumn;
        }

        /// <summary>
        /// Renders a combo box grid view column, representing data from referenced table.
        /// </summary>
        /// <param name="column">Meta data about column, an instance of <see cref="ColumnMeta"/>.</param>
        /// <param name="dataColumn">An instance of <see cref="GridViewEditDataColumn"/>.</param>
        /// <param name="connectionString">Connection string to the underlying database.</param>
        /// <returns></returns>
        private static GridViewEditDataColumn AddGridViewForeignKeyColumn(ColumnMeta column, GridViewEditDataColumn dataColumn, string connectionString)
        {
            var comboBoxColumn = new GridViewDataComboBoxColumn();
            comboBoxColumn.PropertiesComboBox.DataSource = GetLookUpDataSource(column.ReferenceTable, connectionString);
            if (column.ReferenceTable.PrimaryKeys.Length > 1)
                throw new ApplicationException(string.Format("Data source for lookup column {0} has more than one primary key.", column.Name));
            comboBoxColumn.PropertiesComboBox.ValueField = column.ReferenceTable.PrimaryKeys[0].Name;
            comboBoxColumn.PropertiesComboBox.TextField = column.ReferenceTable.LookUpDisplayColumn.Name;
            dataColumn = comboBoxColumn;
            return dataColumn;
        }

        /// <summary>
        /// Obtains grid view data SQL data source based on meta data contained in <see cref="TableMeta"/> and <see cref="ColumnMeta"/>.
        /// </summary>
        /// <param name="tableMeta">Meta data about the table being rendered.</param>
        /// <param name="connectionString">Connection string to the underlying database.</param>
        /// <param name="foreignKey">If detail table, this column provides information about relationship to parent.</param>
        /// <param name="value">This is the value of parent's key, if available.</param>
        /// <returns></returns>
        internal static SqlDataSource GetGridViewDataSource(TableMeta tableMeta, string connectionString, ColumnMeta foreignKey = null, 
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

        /// <summary>
        /// Obtains data source for look up type grid columns.
        /// </summary>
        /// <param name="tableMeta">Meta data about look up table.</param>
        /// <param name="connectionString">Connection string to the underlying database.</param>
        /// <returns>An instance of <see cref="SqlDataSource"/>.</returns>
        internal static SqlDataSource GetLookUpDataSource(TableMeta tableMeta, string connectionString)
        {
            var ds = new SqlDataSource();
            ds.ConnectionString = connectionString;
            ds.SelectCommand = SqlHelper.GenerateSelectQuery(tableMeta);

            return ds;
        }

    }
}
