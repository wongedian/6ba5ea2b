using DevExpress.Web;
using System;
using System.Web.UI;

namespace DotWeb.UI
{
    public class DetailGridTemplate : ITemplate
    {
        Control parent;
        object masterKey;
        TableMetaRelation detailTable;
        TableMeta masterTableMeta;
        string connectionString;

        public DetailGridTemplate(TableMeta masterTableMeta, string connectionString)
        {
            this.masterTableMeta = masterTableMeta;
            if (masterTableMeta.Children.Count != 1)
                throw new ArgumentException(string.Format("Master table {0} has no child table or more than 1 child tables.", masterTableMeta.Name));
            this.detailTable = masterTableMeta.Children[0];
            this.connectionString = connectionString;
        }

        public void InstantiateIn(Control container)
        {
            parent = container;
            masterKey = ((GridViewDetailRowTemplateContainer)parent).KeyValue;
            var gridCreator = new DetailGridCreator(detailTable, masterTableMeta, masterKey, connectionString);
            parent.Controls.Add(new LiteralControl(string.Format("<h3>{0}</h3>", detailTable.Caption)));
            parent.Controls.Add(gridCreator.CreateDetailGrid());
        }
    }
}
