using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using dotless.Core;
using dotless.Core.configuration;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.ServiceAgent;
using System.Security.Claims;

namespace DealnetPortal.Web.Infrastructure
{
    public class DynamicLessHandler : HttpTaskAsyncHandler
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent = DependencyResolver.Current.GetService<IDictionaryServiceAgent>();
        private readonly ILoggingService _loggingService = DependencyResolver.Current.GetService<ILoggingService>();

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            // Load less stylesheet body
            _loggingService.LogInfo("Starting DynamicLessHandler.ProcessRequestAsync");
            var localPath = context.Request.Url.LocalPath;//.Replace(".dynamic", "");
            var fileName = context.Server.MapPath(localPath);
            var fileContent = File.ReadAllText(fileName);

            // Append variable to override
            var sb = new StringBuilder(fileContent);
            //_securityManager.SetUserFromContext();
            var hashDealerName = HttpRequestHelper.GetUrlReferrerRouteDataValues()?["hashDealerName"] as string;
            if (context.User.Identity.IsAuthenticated && !((ClaimsPrincipal)context.User).HasClaim(ClaimTypes.Role, RoleContstants.Dealer)|| hashDealerName != null )
            {
                _loggingService.LogInfo($"Get dealer skinhs settings for {hashDealerName}");
                var variables = await _dictionaryServiceAgent.GetDealerSettings(hashDealerName);
                if (variables != null)
                {
                    _loggingService.LogInfo($"There are  {variables.Count} variables");
                    foreach (var variable in variables)
                    {
                        sb.AppendLine();
                        sb.Append(variable.Name);
                        sb.Append(": ");
                        sb.Append(variable.Value);
                        sb.Append(";");
                    }
                }
            }
            if (hashDealerName != null)
            {
                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                sb.AppendLine($"@logo-img: url('" + urlHelper.Action("LogoImage", "Settings", new { hashDealerName }) + "');");
            }
            // Configure less to allow variable overrides
            var config = DotlessConfiguration.GetDefaultWeb();
            config.DisableVariableRedefines = true;

            // Parse with LESS and write to response stream
            context.Response.ContentType = "text/css";
            context.Response.Write(LessWeb.Parse(sb.ToString(), config));
        }

        public bool IsReusable => true;
    }
}