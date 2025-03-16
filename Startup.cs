using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TraceabilityV3.Startup))]
namespace TraceabilityV3
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
