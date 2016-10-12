using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class MyDealsController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        public MyDealsController(IContractServiceAgent contractServiceAgent, IContractManager contractManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _contractManager = contractManager;
        }
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> ContractEdit(int id)
        {
            ViewBag.EquipmentTypes = (await _contractServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetSummaryAndConfirmationAsync(id));
        }
    }
}