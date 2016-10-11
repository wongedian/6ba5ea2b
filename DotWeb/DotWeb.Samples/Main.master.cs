using DotWeb.UI;
using System;
using System.Web.UI.WebControls;

namespace DotWeb_Samples {

    public partial class MainMaster : System.Web.UI.MasterPage, IMainMaster {
        protected void Page_Load(object sender, EventArgs e) { }

        ContentPlaceHolder IMainMaster.PageTitle
        {
            get { return this.PageTitle; }
        }

        ContentPlaceHolder IMainMaster.MainContent
        {
            get { return this.MainContent; }
        }

    }
}