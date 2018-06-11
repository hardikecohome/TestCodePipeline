using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    public class CustomerFormController : Controller
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ICustomerFormManager _customerFormManager;

        public CustomerFormController(IDictionaryServiceAgent dictionaryServiceAgent, ICustomerFormManager customerFormManager)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _customerFormManager = customerFormManager;
        }
        
        public async Task<ActionResult> Index(string hashDealerName, string culture)
        {
            if (hashDealerName == null || culture == null)
            {
                return RedirectToAction("AnonymousError", "Info");
            }

            var languageOptions = await _dictionaryServiceAgent.GetCustomerLinkLanguageOptions(hashDealerName, culture);
            if (languageOptions == null || !languageOptions.IsLanguageEnabled)
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            ViewBag.ServiceTypes = languageOptions.LanguageServices;
            ViewBag.EnabledLanguages = languageOptions.EnabledLanguages;
            var provinces = await _dictionaryServiceAgent.GetAllProvinceTaxRates();
            if (provinces?.Item1 == null || !provinces.Item1.Any())
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            ViewBag.ProvinceTaxRates = provinces.Item1;
            ViewBag.HashDealerName = hashDealerName;
            return View(new CustomerFormViewModel
                        {
                                DealerName = languageOptions.DealerName,
                                HashDealerName = hashDealerName,
                                IsQuebecDealer = languageOptions.QuebecDealer
                        });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(CustomerFormViewModel customerForm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            var urlBuilder = new UriBuilder(Request.Url.AbsoluteUri)
            {
                Path = Url.Action("ContractEdit", "NewRental"),
                Query = null
            };
            var submitResult = await _customerFormManager.SubmitCustomerForm(customerForm, urlBuilder);

            if (submitResult == null || (submitResult.Item2?.Any(x => x.Type == AlertType.Error) ?? false))
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            return RedirectToAction("AgreementSubmitSuccess", new { contractId = submitResult.Item1?.ContractId, hashDealerName = customerForm.HashDealerName, culture = HttpRequestHelper.GetUrlReferrerRouteDataValues()?["culture"]?.ToString() });
        }

        public async Task<ActionResult> AgreementSubmitSuccess(int contractId, string hashDealerName, string culture)
        {
            var viewModel = await _customerFormManager.GetSubmittedCustomerFormSummary(contractId, hashDealerName, culture);
            var hidePreApprovalAmountsForLeaseDealers = false;
            bool.TryParse(System.Configuration.ConfigurationManager.AppSettings[PortalConstants.HidePreApprovalAmountsForLeaseDealersKey], out hidePreApprovalAmountsForLeaseDealers);
            ViewBag.ShowPreapprovalAmount = !viewModel.IsLeaseTypeDealer || !hidePreApprovalAmountsForLeaseDealers;

            ViewBag.HashDealerName = hashDealerName;
            return View(viewModel);
        }

    }
}