using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Integration;
using DealnetPortal.Api.Models.Enumeration;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    public class DocumentProcessingController : ApiController
    {
        private ILoggingService _loggingService;

        public DocumentProcessingController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpPost]
        public HttpResponseMessage GetDriverLicense(ScanningRequest scanningRequest)
        {
            ImageScanManager scanManager = new ImageScanManager();
            var result = scanManager.ReadDriverLicense(scanningRequest);

            _loggingService.LogInfo("Recieved request for scan driver license");
            
            if (result != null && result.Item2.All(al => al.Type != AlertType.Error))
            {
                _loggingService.LogInfo("Driver license scanned successfully");
                return Request.CreateResponse(HttpStatusCode.OK, result);
            }
            else
            {
                _loggingService.LogError(string.Format("Failed to scan driver license: {0}",
                    result?.Item2.Aggregate(string.Empty, (current, alert) =>
                    current + $"{alert.Header}: {alert.Message};")));
                return Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }
        }

    }
}