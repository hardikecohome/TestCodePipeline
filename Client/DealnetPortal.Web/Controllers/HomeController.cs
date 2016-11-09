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
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ILoggingService _loggingService;

        public HomeController(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent, ILoggingService loggingService)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _loggingService = loggingService;
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
            foreach (var contractsVm in contractsVms)
            {
                try
                {
                    var contract = contracts.FirstOrDefault(x => x.Id == contractsVm.Id);
                    if (contract.Equipment?.NewEquipment == null)
                    {
                        continue;
                    }
                    if (contract.Equipment.AgreementType == AgreementType.LoanApplication)
                    {
                        var provinceCode = contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress)?.State.ToProvinceCode();
                        if (provinceCode == null)
                        {
                            continue;
                        }
                        var taxRate = (await _dictionaryServiceAgent.GetProvinceTaxRate(provinceCode)).Item1;
                        if (taxRate == null)
                        {
                            continue;
                        }
                        var loanCalculatorInput = new LoanCalculator.Input
                        {
                            TaxRate = taxRate.Rate,
                            LoanTerm = contract.Equipment.RequestedTerm,
                            AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                            EquipmentCashPrice = (double?) contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                            AdminFee = contract.Equipment.AdminFee ?? 0,
                            DownPayment = contract.Equipment.DownPayment ?? 0,
                            CustomerRate = contract.Equipment.CustomerRate ?? 0
                        };
                        contractsVm.Value =
                            $"$ {LoanCalculator.Calculate(loanCalculatorInput).TotalBorowingCost:0.00}";
                    }
                    //TODO: Clarify algorithm of Value calculating for non-loan agreement types 
                    //else
                    //{
                    //    contractsVm.Value = $"$ {contract.Equipment.NewEquipment.Sum(e => e.Cost):0.00}";
                    //}
                }
                catch (Exception ex)
                {
                    _loggingService.LogError($"Can't calculate Value value for contract {contractsVm.Id}", ex);
                }
            }
            return this.Json(contractsVms, JsonRequestBehavior.AllowGet);
        }
    }
}