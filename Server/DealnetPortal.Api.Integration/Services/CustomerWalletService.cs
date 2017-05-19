using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.CustomerWallet;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities.Logging;
using OfficeOpenXml.ConditionalFormatting;

namespace DealnetPortal.Api.Integration.Services
{
    public class CustomerWalletService : ICustomerWalletService
    {
        private readonly ICustomerWalletServiceAgent _customerWalletServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly IMailService _mailService;
        private readonly ILoggingService _loggingService;

        public CustomerWalletService(ICustomerWalletServiceAgent customerWalletServiceAgent, IContractRepository contractRepository, ILoggingService loggingService, IMailService mailService)
        {
            _customerWalletServiceAgent = customerWalletServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _mailService = mailService;
        }

        public async Task<IList<Alert>> CreateCustomerByContract(DealnetPortal.Domain.Contract contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            if (contract?.PrimaryCustomer == null)
            {
                throw new NullReferenceException("contract.PrimaryCustomer");
            }
            var customer = contract.PrimaryCustomer;
            var email = customer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                        customer.Emails.FirstOrDefault()?.EmailAddress;
            var randomPassword = System.Web.Security.Membership.GeneratePassword(12, 4);

            var registerCustomer = new RegisterCustomerBindingModel()
            {
                RegisterInfo = new RegisterInfo()
                {
                    Email = email,
                    Password = randomPassword,
                    ConfirmPassword = randomPassword
                },
                Profile = new CustomerProfile()
                {
                    CustomerId = customer.AccountId,
                    DateOfBirth = customer.DateOfBirth,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    EmailAddress = email,
                    Phones = AutoMapper.Mapper.Map<List<PhoneDTO>>(customer.Phones),
                    Locations = AutoMapper.Mapper.Map<List<LocationDTO>>(customer.Locations)
                },
                CreditInfo = new CustomerCreditInfo()
                {
                    DealnetContractId = contract.Id,
                    AspireAccountId = customer.AccountId,
                    CreditAmount = contract.Details.CreditAmount,
                    ScorecardPoints = contract.Details.ScorecardPoints,
                    AspireStatus = contract.Details.Status,
                    AspireTransactionId = contract.Details.TransactionId,
                    UpdateTime = DateTime.Now
                }
            };            

            _loggingService.LogInfo($"Registration new {registerCustomer.RegisterInfo.Email} customer on CustomerWallet portal");
            var submitAlerts = await _customerWalletServiceAgent.RegisterCustomer(registerCustomer);
            if (submitAlerts?.Any() ?? false)
            {
                alerts.AddRange(submitAlerts);
            }
            if (alerts.Any(a => a.Type == AlertType.Error))
            {
                _loggingService.LogInfo($"Failed to register new {registerCustomer.RegisterInfo.Email} on CustomerWallet portal: {alerts.FirstOrDefault(a => a.Type == AlertType.Error)?.Header}");
            }
            //send email notification for DEAL-1490
            else
            {
                await _mailService.SendInviteLinkToCustomer(contract, randomPassword);
            }

            return alerts;
        }
    }
}
