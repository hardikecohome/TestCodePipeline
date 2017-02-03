using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using dotless.Core;
using dotless.Core.configuration;

namespace DealnetPortal.Web.Infrastructure
{
    public class DynamicLessHandler : IHttpHandler
    {
        private const string navBarColor = "#162d43";

        public void ProcessRequest(HttpContext context)
        {
            // Load less stylesheet body
            string localPath = context.Request.Url.LocalPath;//.Replace(".dynamic", "");
            string fileName = context.Server.MapPath(localPath);
            string fileContent = File.ReadAllText(fileName);

            // Append variable to override
            var sb = new StringBuilder(fileContent);
            sb.AppendLine("@navbar-header: " + navBarColor + ";");
            //sb.AppendLine("@gallery-li-bg: " + dbColor2 + ";");

            // Configure less to allow variable overrides
            var config = DotlessConfiguration.GetDefaultWeb();
            config.DisableVariableRedefines = true;

            // Parse with LESS and write to response stream
            context.Response.ContentType = "text/css";
            context.Response.Write(LessWeb.Parse(sb.ToString(), config));
        }

        public bool IsReusable
        {
            get { return true; }
        }
    }
}