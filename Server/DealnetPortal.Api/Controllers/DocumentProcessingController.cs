using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DealnetPortal.Api.Models.Scanning;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    public class DocumentProcessingController : ApiController
    {
        public HttpResponseMessage GetDriverLicense(ScanningRequest scanningRequest)
        {
            throw new NotImplementedException();
        }

    }
}