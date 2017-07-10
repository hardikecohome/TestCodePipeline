﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire.AspireDb;
using DealnetPortal.Api.Models.Contract;

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