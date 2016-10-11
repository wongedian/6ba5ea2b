using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Security;
    using System.Web.SessionState;
    using DevExpress.Web;
using System.Web.Routing;
using DotWeb;

    namespace DotWeb_Admin {
        public class Global_asax : System.Web.HttpApplication {
            void Application_Start(object sender, EventArgs e) {
                DevExpress.Web.ASPxWebControl.CallbackError += new EventHandler(Application_Error);
                RegisterRoutes(RouteTable.Routes);
                InspectSchema();
            }

            void RegisterRoutes(RouteCollection routes)
            {
                routes.MapPageRoute("list",
                    "{module}/list", "~/dynamic/list.aspx");
            }

            void InspectSchema()
            {
                var dbInspector = new DbInspector();
                dbInspector.LoadFromConfig();
                if (dbInspector.SchemaInfo.Tables.Count == 0)
                {
                    dbInspector.GenerateFromDb("AppDb");
                }

                Application["SchemaInfo"] = dbInspector.SchemaInfo;
            }

            void Application_End(object sender, EventArgs e) {
                // Code that runs on application shutdown
            }

            void Application_Error(object sender, EventArgs e) {
                // Code that runs when an unhandled error occurs
            }

            void Session_Start(object sender, EventArgs e) {
                // Code that runs when a new session is started
            }

            void Session_End(object sender, EventArgs e) {
                // Code that runs when a session ends. 
                // Note: The Session_End event is raised only when the sessionstate mode
                // is set to InProc in the Web.config file. If session mode is set to StateServer 
                // or SQLServer, the event is not raised.
            }
        }
    }