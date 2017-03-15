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
using DealnetPortal.Web.Core.Culture;
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
        private readonly ICultureManager _cultureManager;

        public HomeController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent, ICultureManager cultureManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _cultureManager = cultureManager;
        }

        public ActionResult Index()
        {
            ViewBag.LangSwitcherAvailable = true;
            return View("");
        }
        
        public async Task<ActionResult> ChangeCulture(string culture)
        {
            await _cultureManager.ChangeCulture(culture);
            return RedirectToAction("Index");
        }

        public ActionResult About()
        {
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
            var data = summary.Select(s => FormattableString.Invariant($"{s.ItemData:0.00}")).ToList();
            List<object> datasets = new List<object>();
            datasets.Add(new {data});
            return Json(new {labels, datasets}, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetWorkItems(bool? completedOnly)
        {
            var contracts = (completedOnly.HasValue && completedOnly.Value ? await _contractServiceAgent.GetCompletedContracts() : await _contractServiceAgent.GetContracts()).OrderByDescending(x => x.LastUpdateTime).ToList();

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