using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class HomeController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public HomeController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
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
            var data = summary.Select(s => $"{s.ItemData:0.00}").ToList();
            List<object> datasets = new List<object>();
            datasets.Add(new {data});
            return Json(new {labels, datasets}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetWorkItems()
        {
            var contracts = await _contractServiceAgent.GetContracts();
            var contractsVms = AutoMapper.Mapper.Map<IList<DealItemOverviewViewModel>>(contracts);

            var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();

            if (docTypes?.Item1 != null)
            {
                contracts.Where(c => c.ContractState == ContractState.Completed).ForEach(c =>
                {
                    var absentDocs = docTypes.Item1.Where(dt => c.Documents.All(d => dt.Id != d.DocumentTypeId) && !string.IsNullOrEmpty(dt.Prefix)).ToList();
                    if (absentDocs.Any())
                    {
                        var actList = new StringBuilder();
                        absentDocs.ForEach(dt => actList.AppendLine($"{dt.Description};"));
                        var cntrc = contractsVms.FirstOrDefault(cvm => cvm.Id == c.Id);
                        if (cntrc != null)
                        {
                            cntrc.Action = actList.ToString();
                        }
                    }                    
                });
            }
            return this.Json(contractsVms, JsonRequestBehavior.AllowGet);
        }
    }
}