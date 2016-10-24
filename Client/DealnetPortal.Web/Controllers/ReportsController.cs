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

        [HttpGet]
        public async Task<ActionResult> Contract(int id)
        {
            return View(await _contractManager.GetSummaryAndConfirmationAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult> Contracts(IEnumerable<int> ids)
        {
            return View(await _contractManager.GetSummaryAndConfirmationsAsync(ids));
        }

        public async Task<ActionResult> GetXlsxReport(IEnumerable<int> ids)
        {
            var bytes = await _contractServiceAgent.GetXlsxReport(ids);
            return File(bytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace(":", ".")}-report.xlsx");
        }        
    }
}
