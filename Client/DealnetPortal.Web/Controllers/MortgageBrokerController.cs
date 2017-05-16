using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Controllers
{
    public class MortgageBrokerController : Controller
    {
        private readonly ICustomerManager _customerManager;

        public MortgageBrokerController(ICustomerManager customerManager)
        {
            _customerManager = customerManager;
        }

        public async Task<ActionResult> NewClient()
        {
            return View(await _customerManager.GetTemplateAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewClient(NewCustomerViewModel newCustomer)
        {
            var result = await _customerManager.AddAsync(newCustomer);

            if (result?.Any(x => x.Type == AlertType.Error) ?? false)
            {
                TempData[PortalConstants.CurrentAlerts] = result;

                return RedirectToAction("Error", "Info");
            }

            return RedirectToAction("MyClients");
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
        public ActionResult Success()
        {
            return View("CustomerCreationSuccess");
        }

        [HttpGet]
        public ActionResult Decline()
        {
            return View("CustomerCreationDecline");
        }

        [HttpGet]
        public async Task<ActionResult> GetCreatedContracts()
        {
            var result = await _customerManager.GetCreatedContractsAsync();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}