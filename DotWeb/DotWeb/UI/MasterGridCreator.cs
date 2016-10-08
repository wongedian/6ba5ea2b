using DevExpress.Web;
using DotWeb.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    public class MasterGridCreator
    {
        TableMeta tableMeta;
        string connectionString;

        public MasterGridCreator(TableMeta tableMeta, string connectionString)
        {
            this.tableMeta = tableMeta;
            this.connectionString = connectionString;
        }

        public ASPxGridView CreateMasterGrid()
        {
            var masterGrid = new ASPxGridView();
            masterGrid.ID = "masterGrid";
            masterGrid.ClientInstanceName = "masterGrid";
            masterGrid.CssClass = "gridView";
            masterGrid.AutoGenerateColumns = false;
            masterGrid.SettingsPager.PageSize = Constants.DefaultPageSize;
            masterGrid.Paddings.Padding = new Unit("0px");
            masterGrid.Border.BorderWidth = new Unit("0px");
            masterGrid.BorderBottom.BorderWidth = new Unit("1px");
            masterGrid.Settings.ShowGroupPanel = true;
            masterGrid.AutoGenerateColumns = false;
            masterGrid.Columns.Add(GridViewHelper.AddGridViewCommandColumns());
            masterGrid.CustomColumnDisplayText += masterGrid_CustomColumnDisplayText;
            foreach (var column in tableMeta.Columns.OrderBy(c => c.OrderNo))
            {
                if (!column.DisplayInGrid)
                    continue;

                var dataColumn = GridViewHelper.AddGridViewDataColumn(column, connectionString);
                if (dataColumn != null)
                    masterGrid.Columns.Add(dataColumn);
            }
            masterGrid.KeyFieldName = string.Join(";", tableMeta.PrimaryKeys.Select(x => x.Name));
            masterGrid.DataSource = GridViewHelper.GetGridDataSource(tableMeta, connectionString);

            if (tableMeta.Children.Where(c => c.IsRendered).Count() == 1)
            {
                masterGrid.SettingsDetail.ShowDetailRow = true;
                masterGrid.Templates.DetailRow = new DetailGridTemplate(tableMeta, connectionString);
            }
            else if (tableMeta.Children.Where(c => c.IsRendered).Count() > 1)
            {
                masterGrid.SettingsDetail.ShowDetailRow = true;
                masterGrid.Templates.DetailRow = new MultipleDetailGridTemplate(tableMeta, connectionString);
            }

            return masterGrid;
        }

        void masterGrid_CustomColumnDisplayText(object sender, ASPxGridViewColumnDisplayTextEventArgs e)
        {
            var gridView = (sender as ASPxGridView);
            if (e.Column is GridViewDataTextColumn)
            {
                var textColumn = e.Column as GridViewDataTextColumn;
                if (textColumn.PropertiesTextEdit.MaxLength == 0)
                {
                    var cellValue = e.Value.ToString();
                    if (cellValue.Length > Constants.DefaultGridTextColumnMaxLength)
                        e.DisplayText = cellValue.Substring(0, Constants.DefaultGridTextColumnMaxLength) + "...";
                }
            }
        }
    }
}
