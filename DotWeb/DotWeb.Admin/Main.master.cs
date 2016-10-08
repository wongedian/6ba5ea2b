using DotWeb;
using DotWeb.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotWeb_Admin {
    public partial class MainMaster : System.Web.UI.MasterPage, IMainMaster {
        protected void Page_Load(object sender, EventArgs e) { }

        ContentPlaceHolder IMainMaster.MainContent
        {
            get { return this.MainContent; }
        }
    }
}