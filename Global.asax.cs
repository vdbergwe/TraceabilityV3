using System;
using System.Data.Entity.Infrastructure;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using TraceabilityV3.Models;
using Unity;
using Unity.Lifetime;
using Unity.Mvc5;

namespace TraceabilityV3
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static UploadManager UploadManager;

        protected void Application_Start()
        {
            // Register MVC Areas, Filters, Routes, Bundles, and Web API configuration
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // 🔹 Setup Dependency Injection (DI)
            var container = new UnityContainer();

            // ✅ Register TraceabilityDbContextFactory instead of directly injecting TraceabilityEntities
            container.RegisterType<IDbContextFactory<TraceabilityEntities>, TraceabilityDbContextFactory>(new TransientLifetimeManager());

            // 🔹 Register API Client with DI container
            container.RegisterType<ICentralApiClient, CentralAPIClient>(new TransientLifetimeManager());

            // 🔹 Register UploadService (ensuring a new DbContext per call)
            container.RegisterType<UploadService>();

            // 🔹 Set Dependency Resolver for MVC (so Unity is used for dependency injection)
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            // 🔹 Resolve UploadService and create UploadManager instance
            var uploadService = container.Resolve<UploadService>();
            UploadManager = new UploadManager(uploadService);
        }

        protected void Application_End()
        {
            // Clean up when the application ends
            UploadManager?.Dispose();
        }
    }
}
