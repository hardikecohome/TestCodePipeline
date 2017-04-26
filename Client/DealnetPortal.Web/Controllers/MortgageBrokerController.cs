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

    [Authorize]
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
            var submitResult = await _contractServiceAgent.CreateContractForCustomer(newCustomerDto);

            if (submitResult?.Any(x => x.Type == AlertType.Error) ?? false)
            {
                TempData[PortalConstants.CurrentAlerts] = submitResult;
                return RedirectToAction("Error", "Info");
            }
            return RedirectToAction("CustomerCreationSuccess", new { contractId =  0 });
        }

        public ActionResult CustomerCreationSuccess(int id)
        {
            return View();
        }

        public async Task<ActionResult> MyCustomers()
        {
            // TODO: call _contractServiceAgent.GetContracts()) here and then map to ViewModel with customer's information            
            var createdContracts = await _contractServiceAgent.GetCreatedContracts();            

            return View();
        }
    }
}