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
    //[AllowAnonymous]
    public class DocumentProcessingController : ApiController
    {
        private ILoggingService _loggingService;

        //public DocumentProcessingController()
        //{
            
        //}

        public DocumentProcessingController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public IHttpActionResult GetDocument(int id)
        {
            ScanningRequest scanningRequest = new ScanningRequest()
            {
                OperationId = "1",
                ImageForReadRaw = new byte[] {1, 2, 3}
            };
            return Ok(scanningRequest);
        }


        public IHttpActionResult PostDocumentProcessing(ScanningRequest scanningRequest)
        {
            ImageScanManager scanManager = new ImageScanManager();
            var result = scanManager.ReadDriverLicense(scanningRequest);

            _loggingService.LogInfo("Recieved request for scan driver license");
            
            if (result != null && result.Item2.All(al => al.Type != AlertType.Error))
            {
                _loggingService.LogInfo("Driver license scanned successfully");
                return Ok(); ///Request.CreateResponse(HttpStatusCode.OK, result);
            }
            else
            {
                _loggingService.LogError(string.Format("Failed to scan driver license: {0}",
                    result?.Item2.Aggregate(string.Empty, (current, alert) =>
                    current + $"{alert.Header}: {alert.Message};")));
                return BadRequest(); //Request.CreateResponse(HttpStatusCode.BadRequest, result);
            }
        }

    }
}