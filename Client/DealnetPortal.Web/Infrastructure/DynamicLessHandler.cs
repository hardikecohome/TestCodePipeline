using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using dotless.Core;
using dotless.Core.configuration;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Infrastructure.Managers;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class DynamicLessHandler : HttpTaskAsyncHandler
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent = DependencyResolver.Current.GetService<IDictionaryServiceAgent>();
        private readonly ISecurityManager _securityManager = DependencyResolver.Current.GetService<ISecurityManager>();

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            // Load less stylesheet body
            var localPath = context.Request.Url.LocalPath;//.Replace(".dynamic", "");
            var fileName = context.Server.MapPath(localPath);
            var fileContent = File.ReadAllText(fileName);

            // Append variable to override
            var sb = new StringBuilder(fileContent);
            //_securityManager.SetUserFromContext();
            var hashDealerName = HttpRequestHelper.GetUrlReferrerRouteDataValues()?["hashDealerName"] as string;
            if (context.User.Identity.IsAuthenticated || hashDealerName != null)
            {
                var variables = await _dictionaryServiceAgent.GetDealerSettings(hashDealerName);
                if (variables != null)
                {
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