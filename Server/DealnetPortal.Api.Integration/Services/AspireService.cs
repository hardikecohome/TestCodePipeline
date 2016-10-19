using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Aspire;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireService : IAspireService
    {
        private readonly IAspireServiceAgent _aspireServiceAgent;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;

        //Aspire codes
        private const string CodeSuccess = "T000";

        public AspireService(IAspireServiceAgent aspireServiceAgent, IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _aspireServiceAgent = aspireServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
        }

        //TODO: add TransactionId and AccountId support
        public async Task<IList<Alert>> UpdateContractCustomer(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                DealUploadRequest request = new DealUploadRequest();

                var uAlerts = GetAspireUser(request, contractOwnerId);
                if (uAlerts.Any())
                {
                    alerts.AddRange(uAlerts);
                }
                if (alerts.All(a => a.Type != AlertType.Error))
                {
                    FillCustomerInfo(request, contract);

                    try
                    {
                        var response = await _aspireServiceAgent.CustomerUploadSubmission(request).ConfigureAwait(false);
                        var rAlerts = AnalyzeResponse(response, contract);
                        if (rAlerts.Any())
                        {
                            alerts.AddRange(rAlerts);
                        }
                    }
                    catch (Exception ex)
                    {                        
                        alerts.Add(new Alert()
                        {
                            Header = ErrorConstants.AspireConnectionFailed,
                            Type = AlertType.Error,
                            Message = ex.ToString()
                        });
                        _loggingService.LogError("Failed to communicate with Aspire", ex);
                    }                    
                }                
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contractId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contractId}");
            }

            alerts.Where(a => a.Type == AlertType.Error).ForEach(a => 
                _loggingService.LogError($"Aspire issue: {a.Header} {a.Message}"));

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Customers for contract [{contractId}] uploaded to Aspire successfully");
            }

            return alerts;
        }

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId, string contractOwnerId)
        {
            throw new NotImplementedException();
        }

        private IList<Alert> GetAspireUser(DealUploadRequest request, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                request.Header = new Header()
                {
                    From = new From()
                    {
                        AccountNumber = ConfigurationManager.AppSettings["AspireUser"],
                        Password = ConfigurationManager.AppSettings["AspirePassword"]
                    }
                };
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't obtain Aspire user credentials";
                alerts.Add(new Alert()
                {
                    Header = "Can't obtain Aspire user credentials",
                    Message = errorMsg,
                    Type = AlertType.Error
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        private void FillCustomerInfo(DealUploadRequest request, Domain.Contract contract)
        {            
            if (request.Payload == null)
            {
                request.Payload = new Payload();
            }
            if (request.Payload.Lease == null)
            {
                request.Payload.Lease = new Lease();
            }
            if (request.Payload.Lease.Accounts == null)
            {
                request.Payload.Lease.Accounts = new List<Account>();
            }

            if (!string.IsNullOrEmpty(contract.Details?.TransactionId))
            {
                if (request.Payload.Lease.Application == null)
                {
                    request.Payload.Lease.Application = new Application();
                }
                request.Payload.Lease.Application.TransactionId = contract.Details.TransactionId;
            }

            Func<Domain.Customer, Account> fillAccount = c =>
            {
                var account = new Account
                {
                    IsIndividual = true,
                    EmailAddress = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                                   c.Emails?.FirstOrDefault()?.EmailAddress,
                    Personal = new Personal()
                    {
                        Firstname = c.FirstName,
                        Lastname = c.LastName,
                        Dob = c.DateOfBirth.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    }
                };
                if (c.Locations?.Any() ?? false)
                {
                    var loc = c.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress) ??
                              c.Locations.FirstOrDefault();
                    account.Address = new Address()
                    {
                        City = loc.City,
                        Province = new Province()
                        {
                            Abbrev = loc.State.ToProvinceCode()
                        },
                        Postalcode = loc.PostalCode,
                        Country = new Country()
                        {
                            Abbrev = "CAN"
                        },
                        StreetName = loc.Street,
                        SuiteNo = loc.Unit,
                        StreetNo = string.Empty
                    };
                }
                if (c.Phones?.Any() ?? false)
                {
                    var phone = c.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ??
                                c.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ??
                                c.Phones.FirstOrDefault()?.PhoneNum;
                    account.Telecomm = new Telecomm()
                    {
                        Phone = phone
                    };
                }

                if (!string.IsNullOrEmpty(c.AccountId))
                {
                    account.ClientId = c.AccountId;
                }

                return account;
            };

            if (contract.PrimaryCustomer != null)
            {
                var acc = fillAccount(contract.PrimaryCustomer);
                acc.IsPrimary = true;
                request.Payload.Lease.Accounts.Add(acc);
            }

            contract.SecondaryCustomers?.ForEach(c => request.Payload.Lease.Accounts.Add(fillAccount(c)));
        }

        private IList<Alert> AnalyzeResponse(DealUploadResponce responce, Domain.Contract contract)
        {
            var alerts = new List<Alert>();

            if (responce?.Header == null || responce.Header.Status != CodeSuccess || !string.IsNullOrEmpty(responce.Header.ErrorMsg))
            {
                alerts.Add(new Alert()
                {
                    Header = responce.Header.Status,
                    Message = responce.Header.ErrorMsg,
                    Type = AlertType.Error
                });

                if (responce.Payload != null)
                {
                    if (!string.IsNullOrEmpty(responce.Payload.TransactionId) && contract.Details != null && contract.Details.TransactionId != responce.Payload?.TransactionId)
                    {
                        contract.Details.TransactionId = responce.Payload?.TransactionId;
                        _unitOfWork.Save();
                    }

                    if (!string.IsNullOrEmpty(responce.Payload.TransactionId))
                    {
                        
                    }
                }
            }            

            return alerts;
        }
    }
}
