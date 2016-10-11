using DevExpress.Web;
using DevExpress.Web.Data;
using DotWeb.Utils;
using System;
using System.Linq;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    /// <summary>
    /// <para>In a master-detail scenario, this class is responsible for handling detail grid view.</para>
    /// <para>This class provides factory-method to create detail grid view based on meta data passed as arguments.</para>
    /// </summary>
    public class DetailGridCreator
    {
        private ASPxGridView detailGrid;
        private string gridId;
        private object masterKey;
        private TableMeta detailTableMeta;
        private TableMeta masterTableMeta;
        private string connectionString;
        private ColumnMeta foreignKey;

        /// <summary>
        /// Parameterized-constructor for <see cref="DetailGridCreator"/>.
        /// </summary>
        /// <param name="detailTable">An instance of <see cref="TableMetaRelation"/>, provides meta-data about relation
        /// between master and detail tables.</param>
        /// <param name="masterTableMeta">Master table meta data, an instance of <see cref="TableMeta"/>.</param>
        /// <param name="masterKey">The primary key of master table.</param>
        /// <param name="connectionString">Connection string to underlying database.</param>
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

        /// <summary>
        /// The factory method to create detail grid view.
        /// </summary>
        /// <returns>An instance of <see cref="ASPxGridView"/>.</returns>
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
            detailGrid.SettingsBehavior.ConfirmDelete = true;
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
            detailGrid.DataSource = GridViewHelper.GetGridViewDataSource(detailTableMeta, connectionString, foreignKey, masterKey);
            detailGrid.DataBind();
            detailGrid.RowInserting += new ASPxDataInsertingEventHandler(detailGrid_RowInserting);
            detailGrid.RowUpdating += new ASPxDataUpdatingEventHandler(detailGrid_RowUpdating);

            return detailGrid;
        }

        /// <summary>
        /// RowInserting event on grid view. All we need to do here is supplying foreign key value that can be
        /// inferred from master grid view.
        /// </summary>
        /// <param name="sender">Grid view sending the event.</param>
        /// <param name="e">Event args containing new data to be inserted.</param>
        void detailGrid_RowInserting(object sender, DevExpress.Web.Data.ASPxDataInsertingEventArgs e)
        {
            e.NewValues[foreignKey.Name] = (sender as ASPxGridView).GetMasterRowKeyValue();
        }

        /// <summary>
        /// RowUpdating event on grid view. The same with RowInserting, foreign key value can be taken from masteter grid view.
        /// </summary>
        /// <param name="sender">Grid view sending the event.</param>
        /// <param name="e">Event args containing data being updated.</param>
        void detailGrid_RowUpdating(object sender, DevExpress.Web.Data.ASPxDataUpdatingEventArgs e)
        {
            e.NewValues[foreignKey.Name] = (sender as ASPxGridView).GetMasterRowKeyValue();
        }
    }
}
