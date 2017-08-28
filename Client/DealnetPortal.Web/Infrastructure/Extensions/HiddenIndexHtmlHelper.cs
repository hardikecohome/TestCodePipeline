using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DealnetPortal.Web.Infrastructure.Extensions
{
    /// <summary>
    /// Hidden Index Html Helper
    /// </summary>
    public static class HiddenIndexHtmlHelper
    {
        /// <summary>
        /// Hiddens the index for.
        /// </summary>
        /// <typeparam name="TModel">The type of the model.</typeparam>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="expression">The expression.</param>
        /// <param name="index">The Index</param>
        /// <returns>Returns Hidden Index For</returns>
        public static MvcHtmlString HiddenIndexFor<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression, int index)
        {
            var metadata = ModelMetadata.FromLambdaExpression(expression, htmlHelper.ViewData);
            var propName = metadata.PropertyName;

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<input type=\"hidden\" name=\"{0}.Index\" autocomplete=\"off\" value=\"{1}\" />", propName, index);

            return MvcHtmlString.Create(sb.ToString());
        }
    }
}