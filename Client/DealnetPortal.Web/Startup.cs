using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(DealnetPortal.Web.Startup))]
namespace DealnetPortal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
