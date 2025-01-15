using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Http;
using System.Web.Security;

namespace MVCFirebase
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Session_Start()
        {
            Session["sessionid"] = Session.SessionID;
        }
        protected void Application_Start()
        {
            try
            {
                AreaRegistration.RegisterAllAreas();
                System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);
                var formsCookiePath = FormsAuthentication.FormsCookiePath;

            }
            catch (Exception ex)
            {
                //System.IO.File.WriteAllText(Server.MapPath("~/App_Data/ErrorLog.txt"), ex.ToString());
                throw; // Optional: Rethrow to stop the application
            }

        }
    }
}
