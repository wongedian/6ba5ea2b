using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace DotWeb.UI
{
    public class ListPage : System.Web.UI.Page
    {
        protected TableMeta tableMeta;
        protected ASPxGridView masterGrid;
        protected string connectionString;

        public ListPage() : base()
        {
            if (ConfigurationManager.ConnectionStrings["AppDb"] == null)
                throw new ArgumentException("You have to speciry AppDb connection string name in web.config.");
            connectionString = ConfigurationManager.ConnectionStrings["AppDb"].ToString();

            Page.Init += Page_Init;
            Page.Load += Page_Load;
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            var tableName = RouteData.Values["module"] == null ? "" : RouteData.Values["module"].ToString();
            var schemaInfo = Application["SchemaInfo"] as SchemaInfo;

            tableMeta = schemaInfo.Tables.Where(s => s.Name.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (tableMeta == null)
                Response.Redirect("~/404.aspx");
            var gridCreator = new MasterGridCreator(tableMeta, connectionString);
            masterGrid = gridCreator.CreateMasterGrid();
            var masterPage = this.Controls[0] as IMainMaster;
            if (masterPage == null)
                Response.Write("<p>Your master page must implement IMainMaster interface.</p>");
            else
            {
                masterPage.MainContent.Controls.Add(new LiteralControl(string.Format("<h2>{0}</h2>", tableMeta.Caption)));
                masterPage.MainContent.Controls.Add(masterGrid);
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            masterGrid.DataBind();
        }

    }
}
