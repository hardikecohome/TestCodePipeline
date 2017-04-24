using System;
using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Aspire.Integration.Models.AspireDb;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IAspireStorageService
    {
        IList<DropDownItem> GetGenericFieldValues();

        IList<GenericSubDealer> GetSubDealersList(string dealerUserName);

        IList<ContractDTO> GetDealerDeals(string dealerUserName);

        DealerDTO GetDealerInfo(string dealerUserName);

        ContractDTO GetDealById(int transactionId);

        CustomerDTO GetCustomerById(string customerId);

        CustomerDTO FindCustomer(CustomerDTO customer);

        CustomerDTO FindCustomer(string firstName, string lastName, DateTime dateOfBirth, string postalCode);        
    }
}
