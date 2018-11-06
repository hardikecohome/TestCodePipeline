using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class CustomerFormManager : ICustomerFormManager
    {
        private readonly ICustomerFormServiceAgent _customerFormServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public CustomerFormManager(ICustomerFormServiceAgent customerFormServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _customerFormServiceAgent = customerFormServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormViewModel customerForm, UriBuilder urlBuilder)
        {
            var customerFormDto =
                    new CustomerFormDTO {PrimaryCustomer = Mapper.Map<CustomerDTO>(customerForm.HomeOwner)};
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

            if (mainAddress.State.ToUpper() == "QC")
            {
                customerFormDto.PrimaryCustomer.EmploymentInfo =
                        Mapper.Map<EmploymentInfoDTO>(customerForm.HomeOwner.EmploymentInformation);
            }

            var customerContactInfo = Mapper.Map<CustomerDataDTO>(customerForm.HomeOwnerContactInfo);
            customerFormDto.PrimaryCustomer.Emails = customerContactInfo.Emails;
            customerFormDto.PrimaryCustomer.Phones = customerContactInfo.Phones;
            customerFormDto.PrimaryCustomer.AllowCommunicate = customerContactInfo.CustomerInfo.AllowCommunicate;
            customerFormDto.CustomerComment = customerForm.Comment;
            customerFormDto.SelectedService = customerForm.Service;
            customerFormDto.DealerName = customerForm.DealerName;
            customerFormDto.DealUri = urlBuilder.ToString();
            customerFormDto.LeadSource =
                System.Configuration.ConfigurationManager.AppSettings[PortalConstants.CustomerFormLeadSourceKey] ??
                System.Configuration.ConfigurationManager.AppSettings[PortalConstants.DefaultLeadSourceKey];
            var submitResult = await _customerFormServiceAgent.SubmitCustomerForm(customerFormDto);
            return submitResult;
        }

        public async Task<SubmittedCustomerFormViewModel> GetSubmittedCustomerFormSummary(int contractId, string hashDealerName, string culture)
        {
            var languageOptions = await _dictionaryServiceAgent.GetCustomerLinkLanguageOptions(hashDealerName, culture);
            var submitedData = await _customerFormServiceAgent.GetCustomerContractInfo(contractId, languageOptions.DealerName);
            var viewModel = Mapper.Map<SubmittedCustomerFormViewModel>(submitedData);
            
            return viewModel;
        }
    }
}