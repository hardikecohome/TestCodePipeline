using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Core.Culture;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    public class CustomerFormController : Controller
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly ICultureManager _cultureManager;
        public CustomerFormController(IDictionaryServiceAgent dictionaryServiceAgent, IContractServiceAgent contractServiceAgent, ICultureManager cultureManager)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractServiceAgent = contractServiceAgent;
            _cultureManager = cultureManager;
        }

        public async Task<ActionResult> Index(string dealerName, string culture)
        {
            if (dealerName == null || culture == null)
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            var languageOptions = await _dictionaryServiceAgent.GetCustomerLinkLanguageOptions(dealerName, culture);
            if (languageOptions == null || !languageOptions.IsLanguageEnabled)
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            ViewBag.ServiceTypes = languageOptions.LanguageServices;
            ViewBag.EnabledLanguages = languageOptions.EnabledLanguages;
            _cultureManager.SetCulture(culture);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(CustomerFormViewModel customerForm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var customerFormDto = new CustomerFormDTO();
            customerFormDto.PrimaryCustomer = AutoMapper.Mapper.Map<CustomerDTO>(customerForm.HomeOwner);
            customerFormDto.PrimaryCustomer.Locations = new List<LocationDTO>();
            var mainAddress = Mapper.Map<LocationDTO>(customerForm.HomeOwner.AddressInformation);
            mainAddress.AddressType = AddressType.MainAddress;
            customerFormDto.PrimaryCustomer.Locations.Add(mainAddress);
            if (customerForm.HomeOwner.PreviousAddressInformation != null)
            {
                var previousAddress = Mapper.Map<LocationDTO>(customerForm.HomeOwner.PreviousAddressInformation);
                previousAddress.AddressType = AddressType.PreviousAddress;
                customerFormDto.PrimaryCustomer.Locations.Add(previousAddress);
            }
            var customerContactInfo = Mapper.Map<CustomerDataDTO>(customerForm.HomeOwnerContactInfo);
            customerFormDto.PrimaryCustomer.Emails = customerContactInfo.Emails;
            customerFormDto.PrimaryCustomer.Phones = customerContactInfo.Phones;
            customerFormDto.CustomerComment = customerForm.Comment;
            customerFormDto.SelectedService = customerForm.Service;
            customerFormDto.DealerName = customerForm.DealerName;
            var submitResult = await _contractServiceAgent.SubmitCustomerForm(customerFormDto);
            if (submitResult == null || (submitResult.Item2?.Any(x => x.Type == AlertType.Error) ?? false))
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            return RedirectToAction("AgreementSubmitSuccess", new { submitResult.Item1 });
        }

        public ActionResult AgreementSubmitSuccess(CustomerFormResponseDTO submitedData)
        {
            ViewBag.CreditAmount = submitedData.CreditCheck?.CreditAmount;
            ViewBag.DealerName = submitedData.DealerName;
            ViewBag.Street = submitedData.DealerAdress?.Street;
            ViewBag.City = submitedData.DealerAdress?.City;
            ViewBag.Province = submitedData.DealerAdress?.State;
            ViewBag.PostalCode = submitedData.DealerAdress?.PostalCode;
            ViewBag.Phone = submitedData.DealerPhone;
            ViewBag.Email = submitedData.DealerEmail;

            return View();
        }
    }
}