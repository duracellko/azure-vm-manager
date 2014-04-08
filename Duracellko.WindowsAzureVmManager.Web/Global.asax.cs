using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using Duracellko.WindowsAzureVmManager.Web.App_Start;

namespace Duracellko.WindowsAzureVmManager.Web
{
    public class Global : HttpApplication
    {
        public void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            IdentityConfig.ConfigureIdentity();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        public override void Init()
        {
            base.Init();
            this.BeginRequest += this.OnBeginRequest;
        }

        private void OnBeginRequest(object sender, EventArgs e)
        {
            var request = HttpContext.Current.Request;
            if (!request.IsSecureConnection)
            {
                var urlBuilder = new UriBuilder(request.Url);
                urlBuilder.Scheme = Uri.UriSchemeHttps;
                if (urlBuilder.Port == 80)
                {
                    urlBuilder.Port = 443;
                }
                
                var url = urlBuilder.Uri;
                HttpContext.Current.Response.Redirect(url.ToString(), true);
            }
        }
    }
}