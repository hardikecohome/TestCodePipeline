using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Models.Scanning;
namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "MortgageBroker")]
    public class MortgageBrokerController : Controller
    {
        private readonly ICustomerManager _customerManager;
        private readonly IScanProcessingServiceAgent _scanProcessingServiceAgent;
        public MortgageBrokerController(ICustomerManager customerManager, IScanProcessingServiceAgent scanProcessingServiceAgent)
        {
            _customerManager = customerManager;
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
        }

        public async Task<ActionResult> NewClient()
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            return View(await _customerManager.GetTemplateAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewClient(NewCustomerViewModel newCustomer)
        {
            var result = await _customerManager.AddAsync(newCustomer);

            if (result?.Item2.Any(x => x.Type == AlertType.Error) ?? false)
            {
                TempData[PortalConstants.CurrentAlerts] = result;

                return View("CustomerCreationDecline");
            }
            // code need to be change for credit review
            if (result?.Item1 == null || result.Item1.ContractState == ContractState.CreditCheckDeclined || result.Item1.OnCreditReview == true)
            {
                return View("CustomerCreationDecline");
            }

            ViewBag.CreditAmount = Convert.ToInt32(result?.Item1.Details.CreditAmount).ToString("N0", CultureInfo.InvariantCulture);
            ViewBag.FullName = $"{result?.Item1.PrimaryCustomer.FirstName} {result?.Item1.PrimaryCustomer.LastName}";

            return View("CustomerCreationSuccess");
        }

        public ActionResult CustomerCreationSuccess(int id)
        {
            return View();
        }

        public ActionResult MyClients()
        {
            return View();
        }

        
        [HttpGet]
        public async Task<ActionResult> GetCreatedContracts()
        {
            var result = await _customerManager.GetCreatedContractsAsync();

            return Json(result, JsonRequestBehavior.AllowGet);
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
            var result = await _scanProcessingServiceAgent.ScanDriverLicense(scanningRequest);
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
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };
            var result = await _scanProcessingServiceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        protected JsonResult GetSuccessJson()
        {
            return Json(new { isSuccess = true });
        }

        protected JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}