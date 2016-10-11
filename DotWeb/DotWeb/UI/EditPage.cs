using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        protected ASPxGridView masterGrid;
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
        }

        /// <summary>
        /// Page Load event; bind the grid view.
        /// </summary>
        /// <param name="sender">The page sending the event.</param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
        }
    }
}
