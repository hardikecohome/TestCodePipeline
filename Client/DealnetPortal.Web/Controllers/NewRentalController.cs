using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.ServiceAgent;

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
            return View();
        }

        public async Task<ActionResult> TestScanPosting()
        {
            var stream = Request.InputStream;
            string dump;

            using (var reader = new StreamReader(stream))
                dump = reader.ReadToEnd();

            var bytes = String_To_Bytes2(dump);

            var scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = bytes
            };

            var result = await _serviceAgent.ScanDriverLicense(scanningRequest);

            return RedirectToAction("TestLicenseScanning", new { data = result.Item1 });
        }

        public ActionResult TestLicenseScanning(DriverLicenseData data)
        {
            return View(data);
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