using DotWeb;
using System;

namespace DotWeb_Samples {
    public partial class RootMaster : System.Web.UI.MasterPage {
        protected void Page_Load(object sender, EventArgs e) {
            appTitle.Text = (Application["schemaInfo"] as SchemaInfo).App.Name;
            ASPxLabel2.Text = DateTime.Now.Year + Server.HtmlDecode(" &copy; Copyright by [company name]");
        }
    }
}