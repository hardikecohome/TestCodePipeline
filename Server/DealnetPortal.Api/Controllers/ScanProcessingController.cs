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
    //[Authorize]
    public class ScanProcessingController : ApiController
    {
        private ILoggingService _loggingService;

        public ScanProcessingController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }
       
        public IHttpActionResult PostScanProcessing(ScanningRequest scanningRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ImageScanManager scanManager = new ImageScanManager();
            var result = scanManager.ReadDriverLicense(scanningRequest);

            _loggingService.LogInfo("Recieved request for scan driver license");
            
            if (result != null && result.Item2.All(al => al.Type != AlertType.Error))
            {
                _loggingService.LogInfo("Driver license scanned successfully");
                //return Ok(result.Item1);
            }
            else
            {
                var errorMsg = string.Format("Failed to scan driver license: {0}",
                    result?.Item2.Aggregate(string.Empty, (current, alert) =>
                        current + $"{alert.Header}: {alert.Message};"));
                _loggingService.LogError(errorMsg);
                ModelState.AddModelError(ErrorConstants.ScanFailed, errorMsg);
                //return BadRequest();
            }
            return Ok(result);
        }

    }
}