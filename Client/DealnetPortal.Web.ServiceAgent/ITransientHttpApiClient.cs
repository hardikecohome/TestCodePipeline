using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface ITransientHttpApiClient
    {
        IHttpApiClient Client { get; }
        Uri BaseAddress { get; }
    }
}
