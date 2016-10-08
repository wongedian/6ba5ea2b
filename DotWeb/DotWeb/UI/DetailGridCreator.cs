using DevExpress.Web;
using DevExpress.Web.Data;
using DotWeb.Utils;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    public class DetailGridCreator
    {
        ASPxGridView detailGrid;
        string gridId;
        object masterKey;
        TableMeta detailTableMeta;
        TableMeta masterTableMeta;
        string connectionString;
        ColumnMeta foreignKey;

        public DetailGridCreator(TableMetaRelation detailTable, TableMeta masterTableMeta, object masterKey, string connectionString)
        {
            this.detailTableMeta = detailTable.Child;
            this.masterTableMeta = masterTableMeta;
            this.masterKey = masterKey;
            this.connectionString = connectionString;
            this.gridId = string.Concat(detailTableMeta.Name.ToCamelCase(), "GridView");
            this.foreignKey = detailTableMeta.Columns.Where(c => c.IsForeignKey == true && c.Name == detailTable.ForeignKeyName)
                .SingleOrDefault();
            if (foreignKey == null)
                throw new ArgumentException(string.Format("FK to table {0} not found", masterTableMeta.Name));
        }

        public ASPxGridView CreateDetailGrid()
        {
            detailGrid = new ASPxGridView();
            detailGrid.ID = gridId;
            detailGrid.AutoGenerateColumns = false;

            detailGrid.CssClass = "gridView";
            detailGrid.AutoGenerateColumns = false;
            detailGrid.SettingsPager.PageSize = Constants.DefaultPageSize;
            detailGrid.Paddings.Padding = new Unit("0px");
            detailGrid.Border.BorderWidth = new Unit("0px");
            detailGrid.BorderBottom.BorderWidth = new Unit("1px");
            detailGrid.Settings.ShowGroupPanel = true;
            detailGrid.AutoGenerateColumns = false;
            detailGrid.Columns.Add(GridViewHelper.AddGridViewCommandColumns());
            foreach (var column in detailTableMeta.Columns.OrderBy(c => c.OrderNo))
            {
                if (!column.DisplayInGrid || column.Name == foreignKey.Name)
                    continue;

                var dataColumn = GridViewHelper.AddGridViewDataColumn(column, connectionString);
                if (dataColumn != null)
                    detailGrid.Columns.Add(dataColumn);
            }
            detailGrid.KeyFieldName = string.Join(";", detailTableMeta.PrimaryKeys.Select(x => x.Name));
            detailGrid.DataSource = GridViewHelper.GetGridDataSource(detailTableMeta, connectionString, foreignKey, masterKey);
            detailGrid.DataBind();
            detailGrid.RowInserting += new ASPxDataInsertingEventHandler(detailGrid_RowInserting);
            detailGrid.RowUpdating += new ASPxDataUpdatingEventHandler(detailGrid_RowUpdating);

            return detailGrid;
        }

        void detailGrid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            e.NewValues[foreignKey.Name] = (sender as ASPxGridView).GetMasterRowKeyValue();
        }

        void detailGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.NewValues[foreignKey.Name] = (sender as ASPxGridView).GetMasterRowKeyValue();
        }
    }
}
