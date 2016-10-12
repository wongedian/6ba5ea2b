using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
using System.Web.UI;

namespace DotWeb.UI
{
    /// <summary>
    /// <para>Ancestor page for Edit page type.</para>
    /// <para>There are several page types in auto-generated pages: List page type, Edit page type, and Detail page type.</para>
    /// <para>List page type displays several data in grid view, read-write or read only. Edit page type displays single data in
    /// form view, read-write. Detail page type is just like Edit page type, but read-only.</para>
    /// </summary>
    public class EditPage : System.Web.UI.Page
    {
        protected TableMeta tableMeta;
        protected ASPxFormLayout formLayout;
        protected string connectionString;

        public EditPage() : base()
        {
            if (ConfigurationManager.ConnectionStrings["AppDb"] == null)
                throw new ArgumentException("You have to speciry AppDb connection string name in web.config.");
            connectionString = ConfigurationManager.ConnectionStrings["AppDb"].ToString();

            Page.Init += Page_Init;
            Page.Load += Page_Load;
        }

        /// <summary>
        /// Page Init event; creates all required controls in a page.
        /// </summary>
        /// <param name="sender">The page sending the event.</param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            var tableName = RouteData.Values["module"] == null ? "" : RouteData.Values["module"].ToString();
            var schemaInfo = Application["SchemaInfo"] as SchemaInfo;

            tableMeta = schemaInfo.Tables.Where(s => s.Name.Equals(tableName, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (tableMeta == null)
                Response.Redirect("~/404.aspx");

            var routeValues = RouteData.Values["values"] == null ? "" : RouteData.Values["values"].ToString();
            var idValues = routeValues.Split(new char[] { ',' });

            var formLayoutCreator = new FormLayoutCreator(tableMeta, connectionString, idValues, (RouteData.Route as Route).Url.Contains("view"));
            formLayoutCreator.EmptyData += formLayoutCreator_EmptyData;
            formLayout = formLayoutCreator.CreateFormLayout();

            var masterPage = this.Controls[0] as IMainMaster;
            if (masterPage == null)
                Response.Write("<p>Your master page must implement IMainMaster interface.</p>");
            else
            {
                var panel = new System.Web.UI.WebControls.Panel();
                panel.CssClass = "mainContent";
                panel.Controls.Add(new LiteralControl(string.Format("<h2>{0}</h2>", tableMeta.Caption)));
                panel.Controls.Add(formLayout);

                masterPage.MainContent.Controls.Add(panel);
                masterPage.PageTitle.Controls.Add(new LiteralControl(tableMeta.Caption));

            }
        }

        void formLayoutCreator_EmptyData(object sender, EventArgs e)
        {
            formLayout.Visible = false;
            var masterPage = this.Controls[0] as IMainMaster;
            var panel = masterPage.MainContent.Controls[0] as System.Web.UI.WebControls.Panel;
            panel.Controls.Add(new LiteralControl("<p>Data does not exit.</p>"));
        }

        /// <summary>
        /// Page Load event; bind the grid view.
        /// </summary>
        /// <param name="sender">The page sending the event.</param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            formLayout.DataBind();
        }
    }
}
