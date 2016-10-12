using DevExpress.Web;
using DotWeb.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    /// <summary>
    /// This class provides method to create <see cref="ASPxFormLayout"/> for data editing purpose.
    /// </summary>
    public class FormLayoutCreator
    {
        private TableMeta tableMeta;
        private string connectionString;
        private string[] idValues;
        private bool readOnly;

        public event EventHandler EmptyData;

        public FormLayoutCreator(TableMeta tableMeta, string connectionString, string[] idValues, bool readOnly)
        {
            this.tableMeta = tableMeta;
            this.connectionString = connectionString;
            this.idValues = idValues;
            this.readOnly = readOnly;
        }

        public ASPxFormLayout CreateFormLayout()
        {
            var formLayout = new ASPxFormLayout();
            formLayout.ID = "formLayout";
            if (tableMeta.Columns.Count > 20)
                formLayout.ColCount = 3;
            else if (tableMeta.Columns.Count > 5)
                formLayout.ColCount = 2;

            foreach (var column in tableMeta.Columns.OrderBy(c => c.OrderNo))
            {
                if (!column.DisplayInGrid)
                    continue;

                var layoutItem = new LayoutItem(column.Caption);
                layoutItem.FieldName = column.Name;

                ASPxWebControl editor = null;
                if (column.IsPrimaryKey || readOnly)
                {
                    editor = new ASPxLabel();
                }
                else
                    editor = new ASPxTextBox();

                editor.ID = column.Name.RemoveSpaces();
                layoutItem.Controls.Add(editor);
                formLayout.Items.Add(layoutItem);
            }
            var dataSource = GetFormLayoutDataSource(tableMeta, connectionString);
            dataSource.Selected += dataSource_Selected;
            formLayout.DataSource = dataSource;

            return formLayout;
        }

        private SqlDataSource GetFormLayoutDataSource(TableMeta tableMeta, string connectionString)
        {
            var ds = new SqlDataSource();
            ds.ConnectionString = connectionString;
            ds.SelectCommand = SqlHelper.GenerateSelectQuery(tableMeta, true);
            if (tableMeta.PrimaryKeys.Length != idValues.Length)
                throw new ArgumentException("Primary keys count and arguments length are not the same.");
            for (int i = 0; i < idValues.Length; i++)
            {
                ds.SelectParameters.Add(tableMeta.PrimaryKeys[i].Name, idValues[i].ToString());
            }
            ds.UpdateCommand = SqlHelper.GenerateUpdateQuery(tableMeta);
            ds.DeleteCommand = SqlHelper.GenerateDeleteQuery(tableMeta);

            return ds;
        }

        void dataSource_Selected(object sender, SqlDataSourceStatusEventArgs e)
        {
            if (e.AffectedRows == 0)
            {
                OnEmptyData();
            }
        }

        private void OnEmptyData()
        {
            if (EmptyData != null)
                EmptyData(this, new EventArgs());
        }

    }
}
