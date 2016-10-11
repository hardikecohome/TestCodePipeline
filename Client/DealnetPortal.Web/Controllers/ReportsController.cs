using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class ReportsController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        public ReportsController(IContractServiceAgent contractServiceAgent, IContractManager contractManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _contractManager = contractManager;
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Contract(int contractId)
        {
            ViewBag.EquipmentTypes = (await _contractServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }

        [HttpPost]
        public ActionResult Contracts(IEnumerable<int> ids)
        {
            return View();
        }

        public async Task<ActionResult> GetXlsxReport(IEnumerable<int> ids)
        {
            var bytes = await _contractServiceAgent.GetXlsxReport(ids);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace(":", ".")}-report.xlsx");
        }

        public ActionResult ContractEdit()
        {
            return View();
        }        
    }
}
