using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace DotWeb.UI
{
    public class MultipleDetailGridTemplate : ITemplate
    {
        Control parent;
        object masterKey;
        TableMeta masterTableMeta;
        string connectionString;


        public MultipleDetailGridTemplate(TableMeta masterTableMeta, string connectionString)
        {
            this.masterTableMeta = masterTableMeta;
            this.connectionString = connectionString;
        }


        public void InstantiateIn(Control container)
        {
            parent = container;
            masterKey = ((GridViewDetailRowTemplateContainer)parent).KeyValue;

            var pageControl = new ASPxPageControl();
            foreach (var childTableMeta in masterTableMeta.Children)
            {
                var tabPage = new TabPage(childTableMeta.Caption);
                var gridCreator = new DetailGridCreator(childTableMeta, masterTableMeta, masterKey, connectionString);
                tabPage.Controls.Add(gridCreator.CreateDetailGrid());
                pageControl.TabPages.Add(tabPage);
            }
            parent.Controls.Add(pageControl);
        }
    }
}
