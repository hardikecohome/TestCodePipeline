using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class LeadsController : UpdateController
    {
        private readonly IContractServiceAgent _contractServiceAgent;

        public LeadsController(IContractServiceAgent contractServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;
        }

        public async Task<ActionResult> Index()
        {
            //TODO: Map to ViewModels and show to a Dealet
            //var leads = await _contractServiceAgent.GetContractsOffers();
            
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> AcceptLead(int id)
        {
            var result = await _contractServiceAgent.AssignContract(id);

            if (result.Any(x => x.Type == AlertType.Error))
            {
                return GetErrorJson();
            }

            return GetSuccessJson();
        }

    }
}