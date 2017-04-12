using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface ICustomHttpApiClient
    {
        IHttpApiClient Client { get; }
        Uri BaseAddress { get; }
    }
}
