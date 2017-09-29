﻿using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Common.Culture;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Constants;

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
            if (provinces == null || provinces.Item1 == null || !provinces.Item1.Any())
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            ViewBag.ProvinceTaxRates = provinces.Item1;
            ViewBag.HashDealerName = hashDealerName;
            return View(new CustomerFormViewModel { DealerName = languageOptions.DealerName, HashDealerName = hashDealerName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(CustomerFormViewModel customerForm)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("AnonymousError", "Info");
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
            customerFormDto.PrimaryCustomer.AllowCommunicate = customerContactInfo.CustomerInfo.AllowCommunicate;
            customerFormDto.CustomerComment = customerForm.Comment;
            customerFormDto.SelectedService = customerForm.Service;
            customerFormDto.DealerName = customerForm.DealerName;
            var urlBuilder = new UriBuilder(Request.Url.AbsoluteUri)
            {
                Path = Url.Action("ContractEdit", "NewRental"),
                Query = null,
            };
            customerFormDto.DealUri = urlBuilder.ToString();
            customerFormDto.LeadSource = System.Configuration.ConfigurationManager.AppSettings[PortalConstants.DefaultLeadSourceKey];
            var submitResult = await _contractServiceAgent.SubmitCustomerForm(customerFormDto);

            if (submitResult == null || (submitResult.Item2?.Any(x => x.Type == AlertType.Error) ?? false))
            {
                return RedirectToAction("AnonymousError", "Info");
            }
            return RedirectToAction("AgreementSubmitSuccess", new { contractId = submitResult.Item1?.ContractId, hashDealerName = customerForm.HashDealerName, culture = HttpRequestHelper.GetUrlReferrerRouteDataValues()?["culture"]?.ToString() });
        }

        public async Task<ActionResult> AgreementSubmitSuccess(int contractId, string hashDealerName, string culture)
        {
            var languageOptions = await _dictionaryServiceAgent.GetCustomerLinkLanguageOptions(hashDealerName, culture);
            var viewModel = new SubmittedCustomerFormViewModel();
            var submitedData = await _contractServiceAgent.GetCustomerContractInfo(contractId, languageOptions.DealerName);
            viewModel.CreditAmount = submitedData.CreditAmount;
            viewModel.DealerName = submitedData.DealerName;
            viewModel.Street = submitedData.DealerAdress?.Street;
            viewModel.City = submitedData.DealerAdress?.City;
            viewModel.Province = submitedData.DealerAdress?.State;
            viewModel.PostalCode = submitedData.DealerAdress?.PostalCode;
            viewModel.Phone = submitedData.DealerPhone;
            viewModel.Email = submitedData.DealerEmail;
            ViewBag.HashDealerName = hashDealerName;
            return View(viewModel);
        }
    }
}