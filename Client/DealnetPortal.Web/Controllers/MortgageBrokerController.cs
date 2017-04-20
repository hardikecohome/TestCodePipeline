using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{

    public class MortgageBrokerController : Controller
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;

        public MortgageBrokerController(IDictionaryServiceAgent dictionaryServiceAgent, IContractServiceAgent contractServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractServiceAgent = contractServiceAgent;
        }
        public async Task<ActionResult> NewCustomer()
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> NewCustomer(NewCustomerViewModel newCustomer)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
                return View();
            }
            var newCustomerDto = new NewCustomerDTO();
            newCustomerDto.PrimaryCustomer = Mapper.Map<CustomerDTO>(newCustomer.HomeOwner);
            newCustomerDto.PrimaryCustomer.Locations = new List<LocationDTO>();
            var mainAddress = Mapper.Map<LocationDTO>(newCustomer.HomeOwner.AddressInformation);
            mainAddress.AddressType = AddressType.MainAddress;
            newCustomerDto.PrimaryCustomer.Locations.Add(mainAddress);
            if (newCustomer.HomeOwner.PreviousAddressInformation != null)
            {
                var previousAddress = Mapper.Map<LocationDTO>(newCustomer.HomeOwner.PreviousAddressInformation);
                previousAddress.AddressType = AddressType.PreviousAddress;
                newCustomerDto.PrimaryCustomer.Locations.Add(previousAddress);
            }
            var customerContactInfo = Mapper.Map<CustomerDataDTO>(newCustomer.HomeOwnerContactInfo);
            newCustomerDto.PrimaryCustomer.Emails = customerContactInfo.Emails;
            newCustomerDto.PrimaryCustomer.Phones = customerContactInfo.Phones;
            newCustomerDto.CustomerComment = newCustomer.CustomerComment;
            newCustomerDto.HomeImprovementTypes = newCustomer.HomeImprovementTypes;
            var submitResult = await _contractServiceAgent.SubmitNewCustomer(newCustomerDto);

            if (submitResult == null || (submitResult.Item2?.Any(x => x.Type == AlertType.Error) ?? false))
            {
                TempData[PortalConstants.CurrentAlerts] = submitResult?.Item2;
                return RedirectToAction("Error", "Info");
            }
            return RedirectToAction("CustomerCreationSuccess", new { contractId =  0 });
        }

        public ActionResult CustomerCreationSuccess(int id)
        {
            return View();
        }

        public ActionResult MyCustomers()
        {
            return View();
        }
    }
}