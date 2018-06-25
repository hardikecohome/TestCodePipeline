using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using DealnetPortal.Web.ServiceAgent;
using System.Security.Claims;
using DealnetPortal.Web.Common.Constants;

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
            var contract = await _contractManager.GetContractAsync(id);
            var identity = (ClaimsIdentity)User.Identity;
            var isEmcoDealer = identity.HasClaim(ClaimContstants.IsEmcoDealer, "True");
            ViewBag.LeaseProgramDisplayMessage = "";
            if (contract.EquipmentInfo?.RentalProgramType != null && contract.EquipmentInfo?.RentalProgramType == Models.Enumeration.AnnualEscalationType.Escalation35)
            {
                ViewBag.LeaseProgramDisplayMessage = isEmcoDealer ? Resources.Resources.WithEscalatedPayments : Resources.Resources.Escalation_35;
            }
            else if ((contract.EquipmentInfo?.RentalProgramType != null && contract.EquipmentInfo?.RentalProgramType == Models.Enumeration.AnnualEscalationType.Escalation0))
            {
                ViewBag.LeaseProgramDisplayMessage = isEmcoDealer ? Resources.Resources.WithoutEscalatedPayments : Resources.Resources.Escalation_0;
            }

            return View(contract);
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
