using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.ServiceAgent;

using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class LeadsController : UpdateController
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IProfileManager _profileManager;

        public LeadsController(
            IContractServiceAgent contractServiceAgent, 
            IProfileManager profileManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _profileManager = profileManager;
        }

        public async Task<ActionResult> Index()
        {
            //TODO: Map to ViewModels and show to a Dealet
            //var leads = await _contractServiceAgent.GetContractsOffers();

            var result = await _profileManager.GetDealerProfile();

            ViewBag.IsCompletedProfile = result.DealerEquipments != null && result.DealerEquipments.Any() && result.PostalCodes != null && result.PostalCodes.Any();

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AcceptLead(int id)
        {
            var result = await _contractServiceAgent.AssignContract(id);
            var errors = result.Where(x => x.Type == AlertType.Error).Select(a => a.Message).ToList();
            if (errors.Any())
            {
                return Json(new { Errors = errors, isError = true}, JsonRequestBehavior.AllowGet);
            }

            return GetSuccessJson();
        }
    }
}