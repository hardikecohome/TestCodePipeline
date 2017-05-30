using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "MortgageBroker")]
    public class MortgageBrokerController : Controller
    {
        private readonly ICustomerManager _customerManager;

        public MortgageBrokerController(ICustomerManager customerManager)
        {
            _customerManager = customerManager;
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

            if (result?.Item1.ContractState == ContractState.CreditCheckDeclined)
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
    }
}