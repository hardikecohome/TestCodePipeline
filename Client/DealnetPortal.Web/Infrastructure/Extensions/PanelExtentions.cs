using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    public static class PanelExtentions
    {
        public static DpPanel BeginPanel(this HtmlHelper helper)
        {
            return new DpPanel(helper.ViewContext);
        }

        public static void EndPanel(this HtmlHelper helper)
        {
            EndPanel(helper.ViewContext);
        }

        public static void EndPanel(ViewContext context)
        {
            context.Writer.Write("</div>");
            context.OutputClientValidation();
        }
    }
}