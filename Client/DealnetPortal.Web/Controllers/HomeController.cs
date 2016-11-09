using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class HomeController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;

        public HomeController(IContractServiceAgent contractServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;

        }

        public ActionResult Index()
        {
            return View("");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpGet]
        public async Task<ActionResult> GetDealFlowOverview(FlowingSummaryType type)
        {
            var summary = await _contractServiceAgent.GetContractsSummary(type.ToString());
            var labels = summary.Select(s => s.ItemLabel).ToList();
            var data = summary.Select(s => s.ItemData).ToList();
            List<object> datasets = new List<object>();
            datasets.Add(new {data});
            return Json(new {labels, datasets}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetWorkItems()
        {
            var contracts = await _contractServiceAgent.GetContracts();
            var contractsVms = AutoMapper.Mapper.Map<IList<DealItemOverviewViewModel>>(contracts);
            return this.Json(contractsVms, JsonRequestBehavior.AllowGet);
        }
    }
}