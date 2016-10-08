﻿using DevExpress.Web;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DotWeb.UI
{
    [ToolboxData("<{0}:LeftMenu runat=server></{0}:LeftMenu>")]
    public class LeftMenu : ASPxNavBar
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int appId = int.Parse(ConfigurationManager.AppSettings["appId"]);
            var dotWebDb = new DotWebDb();
            var groups = dotWebDb.Groups
                .Include(g => g.App)
                .Include(g => g.Modules)
                .Where(g => g.App.Id == appId && g.ShowInLeftMenu == true)
                .OrderBy(o => o.OrderNo).ToList();
            foreach (var group in groups)
            {
                var navBarGroup = new DevExpress.Web.NavBarGroup(group.Title);
                var modules = group.Modules.Where(m => m.ShowInLeftMenu == true).OrderBy(m => m.OrderNo);
                foreach (var module in modules)
                {
                    var moduleUrl = module.Url;
                    if (module.ModuleType == ModuleType.AutoGenerated)
                        moduleUrl = "~/" + module.TableName + "/list";
                    navBarGroup.Items.Add(new DevExpress.Web.NavBarItem(module.Title, module.Title, null, moduleUrl));
                }
                this.Groups.Add(navBarGroup);
            }
            dotWebDb.Dispose();
        }
    }
}