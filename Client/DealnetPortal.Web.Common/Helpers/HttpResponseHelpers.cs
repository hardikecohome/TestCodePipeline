using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Web.Models;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class HttpResponseHelpers
    {
        public static async Task<IList<Alert>> GetModelStateErrorsAsync(HttpContent content)
        {
            var strContent = await content.ReadAsStringAsync();
            var alerts = new List<Alert>();
            var results =
                        JsonConvert.DeserializeObject<ErrorResponseModel>(strContent);
            if (results?.ModelState != null && results.ModelState.Any())
            {
                results.ModelState.ForEach(st => st.Value.ForEach(val =>
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Message = val,
                        Header = st.Key
                    }))
                );
            }
            return alerts;
        }
    }
}
