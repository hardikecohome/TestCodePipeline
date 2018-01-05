﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using DealnetPortal.Api.Models.Notify;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMailService
    {
        Task<IList<Alert>> SendContractSubmitNotification(ContractDTO contract, string dealerEmail, bool success = true);
        Task<IList<Alert>> SendContractChangeNotification(ContractDTO contract, string dealerEmail);

        /// <summary>
        /// Send e-mail notification to a dealer for a contract created with a customer loan form
        /// </summary>
        /// <param name="dealerEmail"></param>
        /// <param name="customerFormData"></param>
        /// <param name="preapprovedAmount"></param>
        Task SendDealerLoanFormContractCreationNotification(CustomerFormDTO customerFormData, CustomerContractInfoDTO contractData, string dealerProvince);

        /// <summary>
        /// Send e-mail notification to a customer for a contract created with a customer loan form
        /// </summary>
        /// <param name="customerEmail"></param>
        /// <param name="preapprovedAmount"></param>
        /// <param name="dealer"></param>
        /// <param name="dealerColor"></param>
        /// <param name="dealerLogo"></param>
        Task SendCustomerLoanFormContractCreationNotification(string customerEmail, CustomerContractInfoDTO contractData,
            string dealerColor, byte[] dealerLogo);

        Task SendInviteLinkToCustomer(Contract customerFormData, string customerPassword);
        Task SendHomeImprovementMailToCustomer(IList<Contract> customerFormData);
        Task SendCustomerDealerAcceptLead(Contract contract, DealerDTO dealer);
        Task SendNotifyMailNoDealerAcceptLead(Contract contract);
        void SendNotifyMailNoDealerAcceptedLead12H(Contract contract);
        Task SendApprovedMailToCustomer(Contract customerFormData);
        Task SendDeclinedConfirmation(string emailid, string firstName, string lastName);
        Task SendProblemsWithSubmittingOnboarding(string errorMsg, int dealerInfoId, string accessKey);
        Task SendDraftLinkMail(string accessKey, string email);
        Task SendSupportRequiredEmail(SupportRequestDTO SupportDetails, string dealerProvince);
        Task SendDeclineToSign(Contract contract, string dealerProvince);
    }
}
