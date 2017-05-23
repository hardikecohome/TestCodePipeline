using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class CustomerManager : ICustomerManager
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public CustomerManager(
            IDictionaryServiceAgent dictionaryServiceAgent,
            IContractServiceAgent contractServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _contractServiceAgent = contractServiceAgent;
        }

        public async Task<NewCustomerViewModel> GetTemplateAsync()
        {
            var model = new NewCustomerViewModel();

            var equipment = await _dictionaryServiceAgent.GetEquipmentTypes();
            model.EquipmentTypes = equipment.Item1?.OrderBy(x => x.Description).ToList() ??
                                   new List<EquipmentTypeDTO>();

            var taxes = await _dictionaryServiceAgent.GetAllProvinceTaxRates();
            model.ProvinceTaxRates = taxes.Item1 ?? new List<ProvinceTaxRateDTO>();

            var contactMethods = new SelectList(Enum.GetValues(typeof(PreferredContactMethod))
                .Cast<PreferredContactMethod>().Select(v => new SelectListItem
                {
                    Text = v.ToString(),
                    Value = ((int) v).ToString()
                }).ToList(), "Value", "Text");

            model.ContactMethods = contactMethods;

            return model;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> AddAsync(NewCustomerViewModel customer)
        {
            var newCustomerDto = new NewCustomerDTO();
            Mapper.Map(customer, newCustomerDto);

            newCustomerDto.PrimaryCustomer = Mapper.Map<CustomerDTO>(customer.HomeOwner);
            newCustomerDto.PrimaryCustomer.Locations = new List<LocationDTO>();

            var mainAddress = Mapper.Map<LocationDTO>(customer.HomeOwner.AddressInformation);
            mainAddress.AddressType = AddressType.MainAddress;

            newCustomerDto.PrimaryCustomer.Locations.Add(mainAddress);

            if (customer.IsLessThenSix && customer.HomeOwner.PreviousAddressInformation?.City != null)
            {
                var previousAddress = Mapper.Map<LocationDTO>(customer.HomeOwner.PreviousAddressInformation);
                previousAddress.AddressType = AddressType.PreviousAddress;
                newCustomerDto.PrimaryCustomer.Locations.Add(previousAddress);
            }

            if (!customer.IsLiveInCurrentAddress && !customer.IsUnknownAddress && customer.ImprovmentLocation?.City != null)
            {
                var improvmentAddress = Mapper.Map<LocationDTO>(customer.ImprovmentLocation);

                var previousMainAddress = newCustomerDto.PrimaryCustomer.Locations
                    .FirstOrDefault(x => x.AddressType == AddressType.MainAddress);

                if (previousMainAddress != null)
                {
                    previousMainAddress.AddressType = AddressType.PreviousAddress;
                }

                improvmentAddress.AddressType = AddressType.MainAddress;

                newCustomerDto.PrimaryCustomer.Locations.Add(improvmentAddress);
            }

            var customerContactInfo = Mapper.Map<CustomerDataDTO>(customer.HomeOwnerContactInfo);
            newCustomerDto.PrimaryCustomer.Emails = customerContactInfo.Emails;
            newCustomerDto.PrimaryCustomer.Phones = customerContactInfo.Phones;

            return await _contractServiceAgent.CreateContractForCustomer(newCustomerDto);
        }

        public async Task<IList<ClientsInformationViewModel>> GetCreatedContractsAsync()
        {
            var contracts = await _contractServiceAgent.GetCreatedContracts();
            var contractsVms = Mapper.Map<IList<ClientsInformationViewModel>>(contracts);

            return contractsVms
                .OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Id)
                .ToList();
        }
    }
}