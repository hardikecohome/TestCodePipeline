using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Enumeration;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Ajax.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.Controllers
{
    public class NewRentalController : Controller
    {
        private readonly IScanProcessingServiceAgent _serviceAgent;

        public NewRentalController(IScanProcessingServiceAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        public ActionResult BasicInfo()
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            return View();
        }

        [HttpPost]
        public ActionResult BasicInfo(BasicInfoViewModel basicInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            if (!ModelState.IsValid)
            {
                return View();
            }
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> RecognizeDriverLicense(string imgBase64)
        {
            if (imgBase64 == null)
            {
                return GetErrorJson();
            }
            imgBase64 = imgBase64.Replace("data:image/png;base64,", "");
            imgBase64 = imgBase64.Replace(' ', '+');
            var bytes = Convert.FromBase64String(imgBase64);
            var scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = bytes
            };
            var result = await _serviceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }       

        [HttpPost]
        public async Task<JsonResult> RecognizeDriverLicensePhoto()
        {
            if (Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }
            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() {ImageForReadRaw = data};
            var result = await _serviceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        private JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}