using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DealnetPortal.Api.Startup))]

namespace DealnetPortal.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            ConfigurationScheduler(app);
        }
    }
}
