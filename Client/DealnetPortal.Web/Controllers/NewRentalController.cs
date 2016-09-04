using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Ajax.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.Controllers
{
    //[Authorize]
    public class NewRentalController : Controller
    {
        private readonly IScanProcessingServiceAgent _serviceAgent;

        public NewRentalController(IScanProcessingServiceAgent serviceAgent)
        {
            _serviceAgent = serviceAgent;
        }

        public ActionResult BasicInfo()
        {
            return View();
        }

        public async Task<ActionResult> TestScanPosting(string imgBase64)
        {
            if (imgBase64 == null)
            {
                Session["ScannedErrors"] = new List<Alert> {new Alert {Type = AlertType.Error, Header = "noimagedata_submitted", Message = "No image was uploaded"} };
                return RedirectToAction("TestLicenseScanning");
            }
            imgBase64 = imgBase64.Replace("data:image/png;base64,", "");
            imgBase64 = imgBase64.Replace(' ', '+');

            var bytes = Convert.FromBase64String(imgBase64);
            var scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = bytes
            };

            var result = await _serviceAgent.ScanDriverLicense(scanningRequest);
            Session["ScannedInfo"] = result.Item1;
            Session["ScannedErrors"] = result.Item2;

            return RedirectToAction("TestLicenseScanning");
        }

        public async Task<ActionResult> RecognizeDriverLicense()
        {
            ScanningRequest scanningRequest = (ScanningRequest)Session["ScanningRequest"];
            var result = await _serviceAgent.ScanDriverLicense(scanningRequest);
            Session["ScannedInfo"] = result.Item1;
            Session["ScannedErrors"] = result.Item2;
            return RedirectToAction("TestLicenseScanning");
        }        

        [HttpPost]
        public JsonResult UploadDriverLicensePhoto()
        {
            if (Request.Files != null && Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                byte[] data = new byte[file.ContentLength];
                file.InputStream.Read(data, 0, file.ContentLength);

                ScanningRequest scanningRequest = new ScanningRequest() {ImageForReadRaw = data};
                Session["ScanningRequest"] = scanningRequest;
                //var result = _serviceAgent.ScanDriverLicense(scanningRequest).GetAwaiter().GetResult();                
                return Json(new
                {
                    redirectUrl = Url.Action("RecognizeDriverLicense", "NewRental"),
                    isRedirect = true
                });
            }
            return Json(new {isRedirect = false});            
        }


        public ActionResult TestLicenseScanning()
        {
            var driverLicenseData = (DriverLicenseData)Session["ScannedInfo"];
            var errors = (IList<Alert>)Session["ScannedErrors"];
            var driverLicenseViewModel = AutoMapper.Mapper.Map<DriverLicenseViewModel>(new Tuple<DriverLicenseData, IList<Alert>>(driverLicenseData, errors));
            return View(driverLicenseViewModel);
        }

        private byte[] String_To_Bytes2(string strInput)
        {
            int numBytes = (strInput.Length) / 2;
            byte[] bytes = new byte[numBytes];

            for (int x = 0; x < numBytes; ++x)
            {
                bytes[x] = Convert.ToByte(strInput.Substring(x * 2, 2), 16);
            }

            return bytes;
        }
    }
}