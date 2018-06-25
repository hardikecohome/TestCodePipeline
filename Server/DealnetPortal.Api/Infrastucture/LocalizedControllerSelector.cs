using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
using DealnetPortal.Api.Core.Helpers;

namespace DealnetPortal.Api.Infrastucture
{
    public class LocalizedControllerSelector : DefaultHttpControllerSelector
    {
        public LocalizedControllerSelector(HttpConfiguration configuration) : base(configuration)
        {
        }

        public override HttpControllerDescriptor SelectController(HttpRequestMessage request)
        {
            var culture = request.Headers.AcceptLanguage.FirstOrDefault()?.Value;
            if (culture != null)
            {
                Thread.CurrentThread.CurrentCulture =
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureHelper.FilterCulture(culture));
            }

            return base.SelectController(request);
        }
    }
}
