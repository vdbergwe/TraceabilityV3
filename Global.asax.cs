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
using TraceabilityV3.Models;

namespace TraceabilityV3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static TraceabilityEntities DbContext;
        public static UploadManager UploadManager;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Create the EF context.
            DbContext = new TraceabilityEntities();

            // Instantiate your API client.
            ICentralApiClient apiClient = new CentralAPIClient();

            // Create the UploadService.
            var uploadService = new UploadService(DbContext, apiClient);

            // Create the UploadManager (which uses Rx for scheduling).
            UploadManager = new UploadManager(uploadService);
        }

        protected void Application_End()
        {
            // Clean up when the application ends.
            UploadManager?.Dispose();
            DbContext?.Dispose();
        }
    }
}
