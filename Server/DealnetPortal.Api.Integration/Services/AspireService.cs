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
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;
using Application = DealnetPortal.Api.Models.Aspire.Application;

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
                    FillTransactionId(request, contract);
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
                _loggingService.LogInfo($"Customers for contract [{contractId}] uploaded to Aspire successfully with transaction Id [{contract?.Details.TransactionId}]");
            }

            return alerts;
        }

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId, string contractOwnerId)
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
                    FillTransactionId(request, contract);

                    try
                    {
                        var response = await _aspireServiceAgent.CreditCheckSubmission(request).ConfigureAwait(false);
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
                _loggingService.LogInfo($"Aspire credit check for contract [{contractId}] with transaction Id [{contract?.Details.TransactionId}] initiated successfully");
            }

            return alerts;
        }

        public async Task<IList<Alert>> LoginUser(string userName, string password)
        {
            var alerts = new List<Alert>();

            DealUploadRequest request = new DealUploadRequest()
            {
                Header = new Header()
                {
                    UserId = userName,
                    Password = password
                }
            };

            try
            {
                var response = await _aspireServiceAgent.LoginSubmission(request);
                if (response?.Header == null || response.Header.Code != CodeSuccess || !string.IsNullOrEmpty(response.Header.ErrorMsg))
                {
                    alerts.Add(new Alert()
                    {
                        Header = response.Header.Code,
                        Message = response.Header.Message ?? response.Header.ErrorMsg,
                        Type = AlertType.Error
                    });
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
            


            return alerts;
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

        private void InitRequestPayload(DealUploadRequest request)
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
            if (request.Payload.Lease.Application == null)
            {
                request.Payload.Lease.Application = new Application();
            }
        }

        private void FillTransactionId(DealUploadRequest request, Domain.Contract contract)
        {
            InitRequestPayload(request);

            if (!string.IsNullOrEmpty(contract.Details?.TransactionId))
            {
                if (request.Payload.Lease.Application == null)
                {
                    request.Payload.Lease.Application = new Application();
                }
                request.Payload.Lease.Application.TransactionId = contract.Details.TransactionId;
            }
        }

        private void FillCustomerInfo(DealUploadRequest request, Domain.Contract contract)
        {
            const string CustRole = "CUST";
            const string GuarRole = "GUAR";

            InitRequestPayload(request);            

            Func<Domain.Customer, string, Account> fillAccount = (c, role) =>
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

                if (!string.IsNullOrEmpty(role))
                {
                    account.Role = role;
                }

                return account;
            };

            if (contract.PrimaryCustomer != null)
            {
                var acc = fillAccount(contract.PrimaryCustomer, CustRole);
                acc.IsPrimary = true;                
                request.Payload.Lease.Accounts.Add(acc);
            }

            contract.SecondaryCustomers?.ForEach(c => request.Payload.Lease.Accounts.Add(fillAccount(c, GuarRole)));
        }

        private IList<Alert> AnalyzeResponse(DealUploadResponse response, Domain.Contract contract)
        {
            var alerts = new List<Alert>();

            if (response?.Header == null || response.Header.Code != CodeSuccess || !string.IsNullOrEmpty(response.Header.ErrorMsg))
            {
                alerts.Add(new Alert()
                {
                    Header = response.Header.Status,
                    Message = response.Header.Message ?? response.Header.ErrorMsg,
                    Type = AlertType.Error
                });                
            }
            else
            {
                if (response.Payload != null)
                {
                    if (!string.IsNullOrEmpty(response.Payload.TransactionId) && contract.Details != null && contract.Details.TransactionId != response.Payload?.TransactionId)
                    {
                        contract.Details.TransactionId = response.Payload?.TransactionId;
                        _unitOfWork.Save();
                        _loggingService.LogInfo($"Aspire transaction Id [{response.Payload?.TransactionId}] created for contract [{contract.Id}]");
                    }

                    if (response.Payload.Accounts?.Any() ?? false)                        
                    {
                        var idUpdated = false;
                        response.Payload.Accounts.ForEach(a =>
                        {
                            if (a.Name.Contains(contract.PrimaryCustomer.FirstName) &&
                                    a.Name.Contains(contract.PrimaryCustomer.LastName) && contract.PrimaryCustomer.AccountId != a.Id)
                            {
                                contract.PrimaryCustomer.AccountId = a.Id;
                                idUpdated = true;
                            }

                            contract?.SecondaryCustomers.ForEach(c =>
                            {
                                if (a.Name.Contains(c.FirstName) &&
                                    a.Name.Contains(c.LastName) && c.AccountId != a.Id)
                                {
                                    c.AccountId = a.Id;
                                    idUpdated = true;
                                }
                            });
                        });

                        if (idUpdated)
                        {
                            _unitOfWork.Save();
                            _loggingService.LogInfo($"Aspire accounts created for {response.Payload.Accounts.Count} customers");
                        }
                    }
                }
            }      

            return alerts;
        }
    }
}
