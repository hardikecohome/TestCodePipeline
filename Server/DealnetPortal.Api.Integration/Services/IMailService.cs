using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

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
        /// <returns></returns>
        Task<IList<Alert>> SendDealerLoanFormContractCreationNotification(string dealerEmail, CustomerFormDTO customerFormData,
            double? preapprovedAmount);
        /// <summary>
        /// Send e-mail notification to a customer for a contract created with a customer loan form
        /// </summary>
        /// <param name="customerEmail"></param>
        /// <param name="preapprovedAmount"></param>
        /// <param name="dealer"></param>
        /// <param name="dealerColor"></param>
        /// <param name="dealerLogo"></param>
        /// <returns></returns>
        Task<IList<Alert>> SendCustomerLoanFormContractCreationNotification(string customerEmail, double? preapprovedAmount,
            DealerDTO dealer, string dealerColor, byte[] dealerLogo);
    }
}
