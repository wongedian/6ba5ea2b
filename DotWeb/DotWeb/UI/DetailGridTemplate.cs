using DevExpress.Web;
using System;
using System.Web.UI;

namespace DotWeb.UI
{
    /// <summary>
    /// This class is an implementation of ITemplate. Instance of this class can be placed in grid view's DetailRow template.
    /// </summary>
    public class DetailGridTemplate : ITemplate
    {
        private Control parent;
        private object masterKey;
        private TableMetaRelation detailTable;
        private TableMeta masterTableMeta;
        private string connectionString;

        /// <summary>
        /// Parameterized constructor of <see cref="DetailGridTemplate"/>.
        /// </summary>
        /// <param name="masterTableMeta">Meta data of master table.</param>
        /// <param name="connectionString">The connection string to underlying database.</param>
        public DetailGridTemplate(TableMeta masterTableMeta, string connectionString)
        {
            this.masterTableMeta = masterTableMeta;
            if (masterTableMeta.Children.Count != 1)
                throw new ArgumentException(string.Format("Master table {0} has no child table or more than 1 child tables.", masterTableMeta.Name));
            this.detailTable = masterTableMeta.Children[0];
            this.connectionString = connectionString;
        }

        /// <summary>
        /// Creates detail grid view and its header (H3) in a container.
        /// </summary>
        /// <param name="container">The container control in which this template is instantiated.</param>
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
