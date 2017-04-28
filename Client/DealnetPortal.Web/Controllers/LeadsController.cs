using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class LeadsController : Controller
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
    }
}