using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Notify;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Managers;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

using Microsoft.Practices.ObjectBuilder2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using DealnetPortal.Web.Models.Enumeration;
using ContractState = DealnetPortal.Api.Common.Enumeration.ContractState;
using DealnetPortal.Api.Common.Helpers;

namespace DealnetPortal.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly CultureSetterManager _cultureManager;
        private readonly ISettingsManager _settingsManager;
        private readonly IDealerServiceAgent _dealerServiceAgent;
        private readonly IContentManager _contentManager;
        public HomeController(
            IContractServiceAgent contractServiceAgent,
            IDictionaryServiceAgent dictionaryServiceAgent,
            CultureSetterManager cultureManager,
            ISettingsManager settingsManager,
            IDealerServiceAgent dealerServiceAgent,
            IContentManager contentManager)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _cultureManager = cultureManager;
            _settingsManager = settingsManager;
            _dealerServiceAgent = dealerServiceAgent;
            _contentManager = contentManager;
        }

        public ActionResult Index()
        {
            TempData["LangSwitcherAvailable"] = true;
            if(User.IsInRole(RoleContstants.MortgageBroker))
            {
                //just change only for MB release 1.0.6
                TempData["LangSwitcherAvailable"] = false;

                return RedirectToAction("MyClients", "MortgageBroker");
            }
            var identity = (ClaimsIdentity)User.Identity;

            ViewBag.Banner = _contentManager.GetBannerByCulture(CultureInfo.CurrentCulture.Name, identity.HasClaim(ClaimContstants.QuebecDealer, "True"), Request.IsMobileBrowser());

            return View();
        }

        [AllowAnonymous]
        public async Task<ActionResult> ChangeCulture(string culture, string redirectUrl = "")
        {
            await _cultureManager.ChangeCulture(culture);
            if(string.IsNullOrEmpty(redirectUrl))
            {
                return RedirectToAction("Index");
            }
            return Redirect(redirectUrl);
        }

        public async Task<JsonResult> LayoutSettings()
        {
            var aboutAvailability = !(await _settingsManager.CheckDealerSkinExistence(User?.Identity?.Name));
            return Json(new { aboutAvailability }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CustomersDealsCount()
        {
            var contractsCount = await _contractServiceAgent.GetCustomersContractsCount();

            return Json(new { dealsCount = contractsCount }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult About()
        {
            return View();
        }
        public ActionResult Library()
        {
            var identity = (ClaimsIdentity)User.Identity;

            return View(_contentManager.GetResourceFilesByCulture(CultureInfo.CurrentCulture.Name, identity));
        }

        public ActionResult Help()
        {
            return File("~/Content/files/Help.pdf", "application/pdf");
        }

        [HttpGet]
        public async Task<ActionResult> GetDealFlowOverview(FlowingSummaryType type)
        {
            var summary = await _contractServiceAgent.GetContractsSummary(type.ToString());
            var labels = summary.Select(s => s.ItemLabel).ToList();
            var data = summary.Select(s => FormattableString.Invariant($"{s.ItemData:0.00}")).ToList();
            List<object> datasets = new List<object>();
            datasets.Add(new { data });
            return Json(new { labels, datasets }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetWorkItems(bool? completedOnly)
        {
            var contracts =
                (completedOnly ?? false
                    ? await _contractServiceAgent.GetCompletedContracts()
                    : await _contractServiceAgent.GetContracts()).OrderByDescending(x => x.IsNewlyCreated ?? false)
                    .ThenByDescending(x => string.IsNullOrEmpty(x.Details.TransactionId))
                    .ThenByDescending(x => x.LastUpdateTime)
                    .ToList();

            var contractsVms = AutoMapper.Mapper.Map<IList<DealItemOverviewViewModel>>(contracts);

            var tier = await _contractServiceAgent.GetDealerTier();
            var isClarityDealer = ((ClaimsIdentity)User.Identity).HasClaim(ClaimContstants.ClarityDealer, "True");

            contractsVms.ForEach(c =>
            {
                if(string.IsNullOrEmpty(c.AgreementType))
                {
                    return;
                }
                if(c.AgreementType == Models.Enumeration.AgreementType.RentalApplication.GetEnumDescription())
                {
                    c.ProgramOption = Resources.Resources.LeaseApplication;
                    return;
                }
                if(isClarityDealer && (contracts.FirstOrDefault(d => d.Id == c.Id).Equipment?.IsClarityProgram ?? false))
                {
                    c.ProgramOption = Resources.Resources.ClarityProgram;
                    return;
                }
                if(c.AgreementType == Models.Enumeration.AgreementType.LoanApplication.GetEnumDescription() && c.RateCardId.HasValue)
                {
                    var rateCard = tier.RateCards.FirstOrDefault(r => r.Id == c.RateCardId);
                    switch(rateCard?.CardType)
                    {
                        case Api.Common.Enumeration.RateCardType.Custom:
                            c.ProgramOption = Resources.Resources.Custom;
                            break;
                        case Api.Common.Enumeration.RateCardType.FixedRate:
                            c.ProgramOption = c.HasRateReduction ? Resources.Resources.RateReduction : Resources.Resources.StandardRate;
                            break;
                        case Api.Common.Enumeration.RateCardType.NoInterest:
                            c.ProgramOption = Resources.Resources.EqualPayments;
                            break;
                        case Api.Common.Enumeration.RateCardType.Deferral:
                            c.ProgramOption = $"{Convert.ToInt32(rateCard.DeferralPeriod)} {Resources.Resources.Months} {Resources.Resources.Deferral}";
                            break;
                    }
                }
            });

            var identity = (ClaimsIdentity)User.Identity;
            var province = identity.HasClaim(ClaimContstants.QuebecDealer, "True") ? ContractProvince.QC : ContractProvince.ON;
            var provincesDocTypes = await _dictionaryServiceAgent.GetAllStateDocumentTypes();

            if(provincesDocTypes?.Item1 != null)
            {
                contracts.Where(c => c.ContractState == ContractState.Completed).ForEach(c =>
                {
                    var cProvince = c.PrimaryCustomer?.Locations
                                       .FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State
                                   ?? c.PrimaryCustomer?.Locations.FirstOrDefault()?.State;
                    if (!string.IsNullOrEmpty(cProvince))
                    {
                        var docTypes = provincesDocTypes.Item1[cProvince];
                        var absentDocs =
                            docTypes?.Where(
                                    dt => dt.IsMandatory && c.Documents.All(d => dt.Id != d.DocumentTypeId) &&
                                          (dt.Id != 1 || c.Details?.SignatureStatus != SignatureStatus.Completed))
                                .ToList();
                        if (absentDocs?.Any() ?? false)
                        {
                            var actList = new StringBuilder();
                            absentDocs.ForEach(dt => actList.AppendLine($"{dt.Description};"));
                            var cntrc = contractsVms.FirstOrDefault(cvm => cvm.Id == c.Id);
                            if (cntrc != null)
                            {
                                cntrc.Action = actList.ToString();
                            }
                        }
                    }
                });
            }

            return this.Json(contractsVms, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetLeads()
        {
            var contracts = (await _contractServiceAgent.GetLeads()).OrderByDescending(x => x.LastUpdateTime).ToList();
            var contractsVms = AutoMapper.Mapper.Map<IList<DealItemOverviewViewModel>>(contracts);
            //aftermap installation address postalCode
            contracts.Where(c => c.PrimaryCustomer.Locations.Any(l => l.AddressType == AddressType.InstallationAddress)).ForEach(
                c =>
                {
                    var cVms = contractsVms.FirstOrDefault(cvm => cvm.Id == c.Id);
                    if(cVms != null)
                    {
                        var substring = c.PrimaryCustomer.Locations.FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress)?.PostalCode.Substring(0, 3);
                        cVms.PostalCode = $"{substring?.ToUpperInvariant()}***";
                    }
                });
            return Json(contractsVms, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetMaintanenceBanner()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var quebecPrefix = identity.HasClaim(ClaimContstants.QuebecDealer, "True") ? "qc" : string.Empty;

            var pathToView = $@"Maintenance/{CultureInfo.CurrentCulture.Name}/{quebecPrefix}/Banner";

            var viewResult = ViewEngines.Engines.FindView(ControllerContext, pathToView, null);

            if(viewResult.View != null)
            {
                return View(pathToView);
            }
            return Content(string.Empty);
        }

        [HttpGet]
        public PartialViewResult DealerSupportRequestEmail(string contractId)
        {
            var viewModel = new HelpPopUpViewModal();
            if(contractId != "")
            {
                viewModel.LoanNumber = contractId;
            }
            viewModel.DealerName = User.Identity.Name;
            viewModel.YourName = User.Identity.Name;

            return PartialView("_HelpPopUp", viewModel);
        }
        [HttpPost]
        public async Task<string> DealerSupportRequestEmail(HelpPopUpViewModal dealerSupportRequest)
        {
            SupportRequestDTO dealerSupport = new SupportRequestDTO()
            {
                Id = dealerSupportRequest.Id,
                DealerName = dealerSupportRequest.DealerName,
                YourName = dealerSupportRequest.IsPreferedContactPerson ? dealerSupportRequest.PreferedContactPerson : dealerSupportRequest.YourName,
                LoanNumber = dealerSupportRequest.LoanNumber,
                SupportType = dealerSupportRequest.SupportType.ToString(),
                HelpRequested = dealerSupportRequest.HelpRequested,
                BestWay = dealerSupportRequest.BestWayToContact.ToString(),
                ContactDetails = dealerSupportRequest.BestWayToContact == PreferredContactType.Phone ? dealerSupportRequest.Phone : dealerSupportRequest.Email
            };
            var result = await _dealerServiceAgent.DealerSupportRequestEmail(dealerSupport);
            return "ok";
        }
    }
}