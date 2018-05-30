using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class ReportsController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        public ReportsController(IContractServiceAgent contractServiceAgent, IContractManager contractManager, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;
            _contractManager = contractManager;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public ActionResult Index()
        {
            return RedirectToAction("Index", "MyDeals");
        }

        [HttpGet]
        public async Task<ActionResult> Contract(int id)
        {
            return View(await _contractManager.GetContractAsync(id));
        }

        [HttpPost]
        public async Task<ActionResult> Contracts(IEnumerable<int> ids)
        {
            return View(await _contractManager.GetContractsAsync(ids));
        }

        public async Task<ActionResult> GetXlsxReport(IEnumerable<int> ids)
        {
            var report = await _contractServiceAgent.GetXlsxReport(ids, TimeZoneHelper.GetOffset());
            return File(report?.DocumentRaw, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"{DateTime.UtcNow.TryConvertToLocalUserDate().ToString(CultureInfo.CurrentCulture).Replace(":", ".")}-report.xlsx");
        }
    }
}
