using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Constants;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Aspire.Integration.Constants;
using DealnetPortal.Aspire.Integration.Models;
using DealnetPortal.Aspire.Integration.ServiceAgents;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;
using Microsoft.Practices.ObjectBuilder2;
using Application = DealnetPortal.Aspire.Integration.Models.Application;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireService : IAspireService
    {
        private readonly IAspireServiceAgent _aspireServiceAgent;
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly TimeSpan _aspireRequestTimeout;

        //Aspire codes
        private const string CodeSuccess = "T000";

        public AspireService(IAspireServiceAgent aspireServiceAgent, IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, IAspireStorageReader aspireStorageReader, ILoggingService loggingService)
        {
            _aspireServiceAgent = aspireServiceAgent;
            _aspireStorageReader = aspireStorageReader;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _aspireRequestTimeout = TimeSpan.FromSeconds(90);
        }

        public async Task<IList<Alert>> UpdateContractCustomer(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                var result = await UpdateContractCustomer(contract, contractOwnerId);
                if (result?.Any() == true)
                {
                    alerts.AddRange(result);
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

            return alerts;
        }

        public async Task<IList<Alert>> UpdateContractCustomer(Contract contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            CustomerRequest request = new CustomerRequest();

            var userResult = GetAspireUser(contractOwnerId);
            if (userResult.Item2.Any())
            {
                alerts.AddRange(userResult.Item2);
            }
            if (alerts.All(a => a.Type != AlertType.Error))
            {
                request.Header = userResult.Item1;
                request.Payload = new Payload
                {
                    Lease = new Lease()
                    {
                        Application = new Application()
                        {
                            TransactionId = GetTransactionId(contract)
                        },
                        Accounts = GetCustomersInfo(contract)
                    }
                };

                try
                {
                    Task timeoutTask = Task.Delay(_aspireRequestTimeout);
                    var aspireRequestTask = _aspireServiceAgent.CustomerUploadSubmission(request);
                    DecisionCustomerResponse response = null;

                    if (await Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false) == aspireRequestTask)
                    {
                        response = await aspireRequestTask.ConfigureAwait(false);
                    }
                    else
                    {
                        throw new TimeoutException("External system operation has timed out.");
                    }

                    //var response = await _aspireServiceAgent.CustomerUploadSubmission(request).ConfigureAwait(false);

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
                        Code = ErrorCodes.AspireConnectionFailed,
                        Header = ErrorConstants.AspireConnectionFailed,
                        Type = AlertType.Error,
                        Message = ex.ToString()
                    });
                    _loggingService.LogError("Failed to communicate with Aspire", ex);
                }
            }

            alerts.Where(a => a.Type == AlertType.Error).ForEach(a =>
                _loggingService.LogError($"Aspire issue: {a.Header} {a.Message}"));

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Customers for contract [{contract.Id}] uploaded to Aspire successfully with transaction Id [{contract?.Details.TransactionId}]");
            }

            return alerts;
        }

        public async Task<Tuple<CreditCheckDTO, IList<Alert>>> InitiateCreditCheck(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            CreditCheckDTO creditCheckResult = null;            

            if (contract != null)
            {
                if (string.IsNullOrEmpty(contract.Details?.TransactionId))
                {
                    _loggingService.LogWarning($"Aspire transaction wasn't created for contract {contractId} before credit check. Try to create Aspire transaction");
                    //try to call Customer Update for create aspire transaction
                    await UpdateContractCustomer(contractId, contractOwnerId).ConfigureAwait(false);
                }

                if (!string.IsNullOrEmpty(contract.Details?.TransactionId))
                {
                    CreditCheckRequest request = new CreditCheckRequest();

                    var userResult = GetAspireUser(contractOwnerId);
                    if (userResult.Item2.Any())
                    {
                        alerts.AddRange(userResult.Item2);
                    }
                    if (alerts.All(a => a.Type != AlertType.Error))
                    {
                        request.Header = userResult.Item1;

                        request.Payload = new Payload()
                        {
                            TransactionId = contract.Details.TransactionId
                        };

                        try
                        {
                            Task timeoutTask = Task.Delay(_aspireRequestTimeout);
                            var aspireRequestTask = _aspireServiceAgent.CreditCheckSubmission(request);
                            CreditCheckResponse response = null;

                            if (await Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false) == aspireRequestTask)
                            {
                                response = await aspireRequestTask.ConfigureAwait(false);
                            }
                            else
                            {
                                throw new TimeoutException("External system operation has timed out.");
                            }

                            //var response =
                            //    await _aspireServiceAgent.CreditCheckSubmission(request).ConfigureAwait(false);

                            var rAlerts = AnalyzeResponse(response, contract);
                            if (rAlerts.Any())
                            {
                                alerts.AddRange(rAlerts);
                            }
                            if (rAlerts.All(a => a.Type != AlertType.Error))
                            {
                                creditCheckResult = GetCreditCheckResult(response);
                                if (creditCheckResult != null)
                                {
                                    creditCheckResult.ContractId = contractId;
                                }
                            }                            
                        }
                        catch (Exception ex)
                        {
                            alerts.Add(new Alert()
                            {
                                Code = ErrorCodes.AspireConnectionFailed,
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
                        Code = ErrorCodes.AspireTransactionNotCreated,
                        Header = "External system error",
                        Message = $"Can't proceed for credit check for contract {contractId}",
                        Type = AlertType.Error
                    });
                    _loggingService.LogError($"Can't proceed for credit check for contract {contractId}. Aspire transaction should be created first");
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
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

            return new Tuple<CreditCheckDTO, IList<Alert>>(creditCheckResult, alerts);
        }

        public async Task<IList<Alert>> SubmitDeal(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                DealUploadRequest request = new DealUploadRequest();

                var userResult = GetAspireUser(contractOwnerId);
                if (userResult.Item2.Any())
                {
                    alerts.AddRange(userResult.Item2);
                }
                if (alerts.All(a => a.Type != AlertType.Error))
                {
                    // send each equipment separately using same call for avoid Aspire issue
                    for (int i = 0; i < (contract.Equipment?.NewEquipment?.Count ?? 1); i++)
                    {
                        var eqToUpdate = (contract.Equipment?.NewEquipment?.Any() ?? false)
                            ? new List<NewEquipment> {contract.Equipment?.NewEquipment.ElementAt(i)}
                            : null;
                        request.Header = new RequestHeader()
                        {
                            From = new From()
                            {
                                AccountNumber = userResult.Item1.UserId,
                                Password = userResult.Item1.Password
                            }
                        };
                        request.Payload = new Payload()
                        {
                            Lease = new Lease()
                            {
                                Application = GetContractApplication(contract, eqToUpdate)
                            }
                        };

                        try
                        {
                            Task timeoutTask = Task.Delay(_aspireRequestTimeout);
                           
                            var aspireRequestTask = _aspireServiceAgent.DealUploadSubmission(request);
                            DealUploadResponse response = null;

                            if (await Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false) ==
                                aspireRequestTask)
                            {
                                response = await aspireRequestTask.ConfigureAwait(false);
                            }
                            else
                            {
                                throw new TimeoutException("External system operation has timed out.");
                            }

                            var rAlerts = AnalyzeResponse(response, contract, eqToUpdate);
                            if (rAlerts.Any())
                            {
                                alerts.AddRange(rAlerts);
                            }
                        }
                        catch (Exception ex)
                        {
                            alerts.Add(new Alert()
                            {
                                Code = ErrorCodes.AspireConnectionFailed,
                                Header = ErrorConstants.AspireConnectionFailed,
                                Type = AlertType.Error,
                                Message = ex.ToString()
                            });
                            _loggingService.LogError("Failed to communicate with Aspire", ex);
                        }
                    }
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
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
                _loggingService.LogInfo($"Contract [{contractId}] submitted to Aspire successfully with transaction Id [{contract?.Details.TransactionId}]");
            }       

            return alerts;
        }


        public async Task<IList<Alert>> SendDealUDFs(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                var result = await SendDealUDFs(contract, contractOwnerId);
                if (result?.Any() == true)
                {
                    alerts.AddRange(result);
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contractId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contractId}");
            }            

            return alerts;
        }

        public async Task<IList<Alert>> SendDealUDFs(Contract contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            DealUploadRequest request = new DealUploadRequest();

            var userResult = GetAspireUser(contractOwnerId);
            if (userResult.Item2.Any())
            {
                alerts.AddRange(userResult.Item2);
            }
            if (alerts.All(a => a.Type != AlertType.Error))
            {
                request.Header = new RequestHeader()
                {
                    From = new From()
                    {
                        AccountNumber = userResult.Item1.UserId,
                        Password = userResult.Item1.Password
                    }
                };
                request.Payload = new Payload()
                {
                    Lease = new Lease()
                    {
                        Application = GetSimpleContractApplication(contract)
                    }
                };

                try
                {
                    Task timeoutTask = Task.Delay(_aspireRequestTimeout);

                    var aspireRequestTask = _aspireServiceAgent.DealUploadSubmission(request);
                    DealUploadResponse response = null;

                    if (await Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false) ==
                        aspireRequestTask)
                    {
                        response = await aspireRequestTask.ConfigureAwait(false);
                    }
                    else
                    {
                        throw new TimeoutException("External system operation has timed out.");
                    }

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
                        Code = ErrorCodes.AspireConnectionFailed,
                        Header = ErrorConstants.AspireConnectionFailed,
                        Type = AlertType.Error,
                        Message = ex.ToString()
                    });
                    _loggingService.LogError("Failed to communicate with Aspire", ex);
                }
            }

            alerts.Where(a => a.Type == AlertType.Error).ForEach(a =>
                _loggingService.LogError($"Aspire issue: {a.Header} {a.Message}"));

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Contract [{contract.Id}] submitted to Aspire successfully with transaction Id [{contract?.Details.TransactionId}]");
            }

            return alerts;
        }

        public async Task<IList<Alert>> UploadDocument(int contractId, ContractDocumentDTO document,
            string contractOwnerId)
        {
            var alerts = new List<Alert>();

            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null && document?.DocumentBytes != null)
            {                
                var docTypeId = document.DocumentTypeId;
                var docTypes = _contractRepository.GetDocumentTypes();

                var docType = docTypes?.FirstOrDefault(t => t.Id == docTypeId);
                if (!string.IsNullOrEmpty(docType?.Prefix))
                {
                    if (string.IsNullOrEmpty(document.DocumentName) || !document.DocumentName.StartsWith(docType.Prefix))
                    {
                        document.DocumentName = docType.Prefix + document.DocumentName;
                    }
                }

                var request = new DocumentUploadRequest();

                var userResult = GetAspireUser(contractOwnerId);
                if (userResult.Item2.Any())
                {
                    alerts.AddRange(userResult.Item2);
                }
                if (alerts.All(a => a.Type != AlertType.Error))
                {
                    request.Header = userResult.Item1;

                    try
                    {                    
                        request.Payload = new DocumentUploadPayload()
                        {
                            TransactionId = contract.Details.TransactionId,                            
                            
                            Status = ConfigurationManager.AppSettings[WebConfigKeys.DOCUMENT_UPLOAD_STATUS_CONFIG_KEY]                            
                        };

                        request.Payload.Documents = new List<Document>()
                        {
                            new Document()
                            {
                                Name = Path.GetFileNameWithoutExtension(document.DocumentName), 
                                Data = Convert.ToBase64String(document.DocumentBytes),
                                Ext = Path.GetExtension(document.DocumentName)?.Substring(1)
                            }
                        };

                        var docUploadResponse = await _aspireServiceAgent.DocumentUploadSubmission(request).ConfigureAwait(false);
                        var rAlerts = AnalyzeResponse(docUploadResponse, contract);
                        if (rAlerts.Any())
                        {
                            alerts.AddRange(rAlerts);
                        }

                        if (rAlerts.All(a => a.Type != AlertType.Error))
                        {
                            _loggingService.LogInfo($"Document {document.DocumentName} to Aspire for contract {contractId} was uploaded successfully");
                        }                        
                    }
                    catch (Exception ex)
                    {
                        alerts.Add(new Alert()
                        {
                            Code = ErrorCodes.AspireConnectionFailed,
                            Header = "Can't upload document",
                            Message = ex.ToString(),
                            Type = AlertType.Error
                        });
                        _loggingService.LogError($"Can't upload document to Aspire for contract {contractId}", ex);
                    }
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contractId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contractId}");
            }

            return alerts;
        }

        public async Task<IList<Alert>> SubmitAllDocumentsUploaded(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                if (new[] { (int) DocumentTemplateType.SignedContract, (int) DocumentTemplateType.SignedInstallationCertificate, 3, 4}.
                    All(x => contract.Documents.Any(d => d.DocumentTypeId == x)))
                {
                    var request = new DocumentUploadRequest();

                    var userResult = GetAspireUser(contractOwnerId);
                    if (userResult.Item2.Any())
                    {
                        alerts.AddRange(userResult.Item2);
                    }
                    if (alerts.All(a => a.Type != AlertType.Error))
                    {
                        request.Header = userResult.Item1;

                        try
                        {
                            request.Payload = new DocumentUploadPayload()
                            {
                                TransactionId = contract.Details.TransactionId,
                                Status = ConfigurationManager.AppSettings[WebConfigKeys.ALL_DOCUMENTS_UPLOAD_STATUS_CONFIG_KEY]
                            };

                            var submitString = "Request to Fund";
                            var submitStrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(submitString));
                            request.Payload.Documents = new List<Document>()
                            {
                                new Document()
                                {
                                    Name = "ReadyForAudit",
                                    Data = submitStrBase64,
                                    Ext = "txt"
                                }
                            };

                            var docUploadResponse =
                                await _aspireServiceAgent.DocumentUploadSubmission(request).ConfigureAwait(false);
                            var rAlerts = AnalyzeResponse(docUploadResponse, contract);
                            if (rAlerts.Any())
                            {
                                alerts.AddRange(rAlerts);
                            }

                            //if (contract.ContractState != ContractState.SentToAudit)
                            //{
                            //    alerts.Add(new Alert()
                            //    {
                            //        Header = "Deal wasn't sent to audit",
                            //        Message = $"Deal wasn't sent to audit for contract with id {contractId}",
                            //        Type = AlertType.Error
                            //    });
                            //    _loggingService.LogError($"Deal wasn't sent to audit for contract with id {contractId}");
                            //}

                            if (rAlerts.All(a => a.Type != AlertType.Error))
                            {
                                contract.ContractState = ContractState.SentToAudit;
                                _unitOfWork.Save();
                                _loggingService.LogInfo(
                                    $"All Documents Uploaded Request was successfully sent to Aspire for contract {contractId}");
                            }
                        }
                        catch (Exception ex)
                        {
                            alerts.Add(new Alert()
                            {
                                Code = ErrorCodes.AspireConnectionFailed,
                                Header = "Can't upload document",
                                Message = ex.ToString(),
                                Type = AlertType.Error
                            });
                            _loggingService.LogError($"Can't upload document to Aspire for contract {contractId}", ex);
                        }
                    }
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Header = "Not all mandatory documents were uploaded",
                        Message = $"Not all mandatory documents were uploaded for contract with id {contractId}",
                        Type = AlertType.Error
                    });
                    _loggingService.LogError($"Not all mandatory documents were uploaded for contract with id {contractId}");
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contractId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contractId}");
            }

            return alerts;
        }

        public async Task<IList<Alert>> LoginUser(string userName, string password)
        {
            var alerts = new List<Alert>();

            DealUploadRequest request = new DealUploadRequest()
            {
                Header = new RequestHeader()
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
                    Code = ErrorCodes.AspireConnectionFailed,
                    Header = ErrorConstants.AspireConnectionFailed,
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });
                _loggingService.LogError("Failed to communicate with Aspire", ex);
            }
            


            return alerts;
        }

        #region private      

        private Tuple<RequestHeader, IList<Alert>> GetAspireUser(string contractOwnerId)
        {
            var alerts = new List<Alert>();
            RequestHeader header = null;            

            try
            {
                var dealer = _contractRepository.GetDealer(contractOwnerId);
                if (dealer != null && !string.IsNullOrEmpty(dealer.AspireLogin) &&
                    !string.IsNullOrEmpty(dealer.AspirePassword))
                {
                    header = new RequestHeader()
                    {
                        UserId = dealer.AspireLogin,
                        Password = dealer.AspirePassword
                    };
                }
                else
                {
                    header = new RequestHeader()
                    {
                        UserId = ConfigurationManager.AppSettings[WebConfigKeys.ASPIRE_USER_CONFIG_KEY],
                        Password = ConfigurationManager.AppSettings[WebConfigKeys.ASPIRE_PASSWORD_CONFIG_KEY]
                    };
                }
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't obtain user credentials";
                alerts.Add(new Alert()
                {
                    Header = errorMsg,
                    Message = errorMsg,
                    Type = AlertType.Error
                });
                _loggingService.LogError("Can't obtain Aspire user credentials");
            }

            return new Tuple<RequestHeader, IList<Alert>>(header, alerts);
        }        

        private string GetTransactionId(Domain.Contract contract)
        {
            return contract?.Details?.TransactionId;            
        }

        private List<Account> GetCustomersInfo(Domain.Contract contract)
        {
            const string CustRole = "CUST";
            const string GuarRole = "GUAR";

            var accounts = new List<Account>();
            var portalDescriber = ConfigurationManager.AppSettings[$"PortalDescriber.{contract.Dealer?.ApplicationId}"];            

            Func<Domain.Customer, string, Account> fillAccount = (c, role) =>
            {
                var account = new Account
                {
                    IsIndividual = true,
                    IsPrimary = true,
                    Legalname = contract.Dealer?.Application?.LegalName,
                    EmailAddress = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                                   c.Emails?.FirstOrDefault()?.EmailAddress,
                    CreditReleaseObtained = true,
                    Personal = new Personal()
                    {
                        Firstname = c.FirstName,
                        Lastname = c.LastName,
                        Dob = c.DateOfBirth.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    },
                };
                
                var location = c.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress) ??
                                      c.Locations?.FirstOrDefault();
                if (location == null)
                {
                    // try to get primary customer location
                    location = contract.PrimaryCustomer?.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress) ??
                                      contract.PrimaryCustomer?.Locations?.FirstOrDefault();
                }

                if (location != null)
                {                    
                    account.Address = new Address()
                    {
                        City = location.City,
                        Province = new Province()
                        {
                            Abbrev = location.State.ToProvinceCode()
                        },
                        Postalcode = location.PostalCode,
                        Country = new Country()
                        {
                            Abbrev = "CAN"
                        },
                        StreetName = location.Street,
                        SuiteNo = location.Unit,
                        StreetNo = string.Empty
                    };                    
                }
                
                if (c.Phones?.Any() ?? false)
                {
                    var phone = c.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ??
                                c.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ??
                                c.Phones.FirstOrDefault()?.PhoneNum;
                    account.Telecomm = new Telecomm()
                    {
                        Phone = phone
                    };
                }
                if (c.Emails?.Any() ?? false)
                {
                    var email = c.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                                c.Emails.FirstOrDefault()?.EmailAddress;
                    if (account.Telecomm == null)
                    {
                        account.Telecomm = new Telecomm();
                    }
                    account.Telecomm.Email = email;
                }

                if (string.IsNullOrEmpty(c.AccountId))
                {
                    //check user on Aspire
                    var postalCode =
                        c.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.PostalCode ??
                        c.Locations?.FirstOrDefault()?.PostalCode;
                    try
                    {                    
                        var aspireCustomer = AutoMapper.Mapper.Map<CustomerDTO>(_aspireStorageReader.FindCustomer(c.FirstName, c.LastName, c.DateOfBirth, postalCode));
                        if (aspireCustomer != null)
                        {
                            account.ClientId = aspireCustomer.AccountId?.Trim();
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("Failed to get customer from Aspire", ex);
                        account.ClientId = null;
                    }
                }
                else
                {
                    account.ClientId = c.AccountId;
                } 
                
                account.UDFs = GetCustomerUdfs(c, location, portalDescriber, contract.HomeOwners?.Any(hw => hw.Id == c.Id)).ToList();                

                if (!string.IsNullOrEmpty(role))
                {
                    account.Role = role;
                }                                

                return account;
            };

            if (contract.PrimaryCustomer != null)
            {
                var acc = fillAccount(contract.PrimaryCustomer, CustRole);
                //acc.IsPrimary = true;                
                accounts.Add(acc);
            }

            contract.SecondaryCustomers?.ForEach(c => accounts.Add(fillAccount(c, GuarRole)));
            return accounts;
        }

        private Application GetContractApplication(Domain.Contract contract, ICollection<NewEquipment> newEquipments = null)
        {
            var application = new Application()
            {
                TransactionId = contract.Details?.TransactionId
            };

            var pTaxRate = _contractRepository.GetProvinceTaxRate(contract.PrimaryCustomer.Locations.FirstOrDefault().State);

            if (contract.Equipment != null)
            {
                var equipments = newEquipments ?? contract.Equipment.NewEquipment;
                equipments?.ForEach(eq =>
                {
                    if (application.Equipments == null)
                    {
                        application.Equipments = new List<Equipment>();
                    }
                    application.Equipments.Add(new Equipment()
                    {
                        Status = "new",
                        AssetNo = string.IsNullOrEmpty(eq.AssetNumber) ? null : eq.AssetNumber,
                        Quantity = "1",
                        Cost = contract.Equipment.AgreementType == AgreementType.LoanApplication && eq.Cost.HasValue ? (eq.Cost + Math.Round(((decimal)(eq.Cost / 100 * (decimal)(pTaxRate.Rate))), 2))?.ToString(CultureInfo.InvariantCulture) 
                                                                                                    : eq.MonthlyCost?.ToString(CultureInfo.InvariantCulture),
                        Description = eq.Description,
                        AssetClass = new AssetClass() { AssetCode = eq.Type }
                    });
                });
                application.AmtRequested = contract.Equipment.AmortizationTerm?.ToString();
                application.TermRequested = contract.Equipment.RequestedTerm.ToString();
                application.Notes = contract.Details?.Notes ?? contract.Equipment.Notes;
                //TODO: Implement finance program selection
                application.FinanceProgram = contract.Dealer?.Application?.FinanceProgram;//"EcoHome Finance Program";

                application.ContractType = contract.Equipment?.AgreementType == AgreementType.LoanApplication
                    ? "LOAN"
                    : "RENTAL";                
            }
            application.UDFs = GetApplicationUdfs(contract).ToList();

            return application;
        }

        /// <summary>
        /// UDFs and some basic info only
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        private Application GetSimpleContractApplication(Domain.Contract contract)
        {
            var application = new Application()
            {
                TransactionId = contract.Details?.TransactionId,                
            };

            if (contract.Equipment != null)
            {                
                application.AmtRequested = contract.Equipment.AmortizationTerm?.ToString();
                application.TermRequested = contract.Equipment.RequestedTerm.ToString();

                application.ContractType = contract.Equipment?.AgreementType == AgreementType.LoanApplication
                    ? "LOAN"
                    : "RENTAL";                
            }

            application.Notes = contract.Details?.Notes ?? contract.Equipment?.Notes;
            //TODO: Implement finance program selection
            application.FinanceProgram = contract.Dealer?.Application?.FinanceProgram;//"EcoHome Finance Program";
            application.UDFs = GetApplicationUdfs(contract).ToList();

            return application;
        }

        private IList<Alert> AnalyzeResponse(DealUploadResponse response, Domain.Contract contract, ICollection<NewEquipment> newEquipments = null)
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

                    if (!string.IsNullOrEmpty(response.Payload.ContractStatus) && contract.Details != null &&
                        contract.Details.Status != response.Payload.ContractStatus)
                    {
                        contract.Details.Status = response.Payload.ContractStatus;
                        var aspireStatus = _contractRepository.GetAspireStatus(response.Payload.ContractStatus);
                        if (aspireStatus != null && aspireStatus.Interpretation.HasFlag(AspireStatusInterpretation.SentToAudit) &&
                            contract.ContractState != ContractState.SentToAudit)
                        {
                            contract.ContractState = ContractState.SentToAudit;
                            contract.LastUpdateTime = DateTime.Now;
                        }
                        _unitOfWork.Save();
                        _loggingService.LogInfo($"Contract [{contract.Id}] state was changed to [{response.Payload.ContractStatus}]");
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

                    if (response.Payload.Asset != null)
                    {
                        var eqCollection = newEquipments ?? contract?.Equipment?.NewEquipment;
                        var aEq = eqCollection?.FirstOrDefault(
                                eq => eq.Description == response.Payload.Asset.Name);
                        if (aEq != null)
                        {
                            aEq.AssetNumber = response.Payload.Asset.Number;
                            _unitOfWork.Save();
                            _loggingService.LogInfo($"Aspire asset number {response.Payload.Asset.Number} assigned for equipment for contract {contract.Id}");
                        }
                    }
                }
            }      

            return alerts;
        }

        private CreditCheckDTO GetCreditCheckResult(DealUploadResponse response)
        {
            CreditCheckDTO checkResult = new CreditCheckDTO();

            int scorePoints = 0;

            if (!string.IsNullOrEmpty(response?.Payload?.ScorecardPoints) &&
                int.TryParse(response.Payload.ScorecardPoints, out scorePoints))
            {
                checkResult.ScorecardPoints = scorePoints;
                if (scorePoints > 180 && scorePoints <= 220)
                {
                    checkResult.CreditAmount = 15000;
                }
                if (scorePoints > 220 && scorePoints < 1000)
                {
                    checkResult.CreditAmount = 20000;
                }
            }

            checkResult.CreditCheckState = CreditCheckState.Initiated;

            bool passFail = true;
            bool.TryParse(response?.Payload?.ScorecardPassFail, out passFail);

            const string Approved = "Approved";
            const string Declined = "Declined";
            string[] CreditReview = { "Credit Review", "MIR", "Submitted" };

            if (!string.IsNullOrEmpty(response?.Payload?.ContractStatus) && !passFail)
            {
                if (response.Payload.ContractStatus.Contains(Approved))
                {
                    checkResult.CreditCheckState = CreditCheckState.Approved;
                }
                if (response.Payload.ContractStatus.Contains(Declined))
                {
                    checkResult.CreditCheckState = CreditCheckState.Declined;
                }
                if (CreditReview.Any(c => response.Payload.ContractStatus.Contains(c)))
                {
                    checkResult.CreditCheckState = CreditCheckState.MoreInfoRequired;
                }
            }

            return checkResult;
        }

        private IList<UDF> GetApplicationUdfs(Domain.Contract contract)
        {
            var udfList = new List<UDF>();
            if (contract?.Equipment != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.DeferralType,
                    Value = contract.Equipment.DeferralType.GetPersistentEnumDescription()
                });
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.RequestedTerm,
                    Value = contract.Equipment.RequestedTerm.ToString()
                });

                if (!string.IsNullOrEmpty(contract.Equipment.SalesRep))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.DealerSalesRep,
                        Value = contract.Equipment.SalesRep
                    });
                }
                if (contract.Equipment.EstimatedInstallationDate.HasValue)
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.PreferredInstallationDate,
                        Value = contract.Equipment.EstimatedInstallationDate.Value.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    });
                }
                if (contract.Equipment.PreferredStartDate.HasValue)
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.PreferredDateToStartProject,
                        Value = contract.Equipment.PreferredStartDate.Value.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    });
                }
                if (contract.Equipment.NewEquipment?.Any() == true)
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.HomeImprovementType,
                        Value = contract.Equipment.NewEquipment.First().Type
                    });
                }
            }

            if (contract?.PaymentInfo != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.PaymentType,
                    Value = contract.PaymentInfo?.PaymentType == PaymentType.Enbridge ? "Enbridge" : "PAD"
                });
                if (contract.PaymentInfo?.PaymentType == PaymentType.Enbridge &&
                    (!string.IsNullOrEmpty(contract.PaymentInfo?.EnbridgeGasDistributionAccount) ||
                    !string.IsNullOrEmpty(contract.PaymentInfo?.MeterNumber)))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.EnbridgeGasAccountNumber,
                        Value = contract.PaymentInfo.EnbridgeGasDistributionAccount ?? contract.PaymentInfo.MeterNumber
                    });
                }
            }

            if (!string.IsNullOrEmpty(contract?.ExternalSubDealerId))
            {
                try
                {
                    var subDealers =
                        _aspireStorageReader.GetSubDealersList(contract.Dealer.AspireLogin ?? contract.Dealer.UserName);
                    var sbd = subDealers?.FirstOrDefault(sd => sd.SubmissionValue == contract.ExternalSubDealerId);
                    if (sbd != null)
                    {
                        udfList.Add(new UDF()
                        {
                            Name = sbd.DealerName,
                            Value = sbd.SubmissionValue
                        });
                    }
                }
                catch (Exception ex)
                {
                    //we can get error here from Aspire DB
                    _loggingService.LogError("Failed to get subdealers from Aspire", ex);
                }
            }
            var portalDescriber = ConfigurationManager.AppSettings[$"PortalDescriber.{contract.Dealer?.ApplicationId}"];
            if (!string.IsNullOrEmpty(portalDescriber))
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadSource,
                    Value = portalDescriber
                });
            }

            return udfList;
        }

        private IList<UDF> GetCustomerUdfs(Domain.Customer customer, Location mainLocation, string portalDescriber, bool? isHomeOwner = null)
        {
            var udfList = new List<UDF>();
            if (!string.IsNullOrEmpty(portalDescriber))
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadSource,
                    Value = portalDescriber
                });
            }
            if (mainLocation != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.Residence,
                    Value = mainLocation.ResidenceType == ResidenceType.Own ? "O" : "R"
                    //<!—other value is R for rent  and O for own-->
                });
            }
            udfList.Add(
                new UDF()
                {

                    Name = AspireUdfFields.AuthorizedConsent,
                    Value = "Y"
                });
            udfList.Add(
                new UDF()
                {

                    Name = AspireUdfFields.ExistingCustomer,
                    Value = string.IsNullOrEmpty(customer.AccountId) ? "N" : "Y" // ???
                });

            var previousAddress = customer.Locations?.FirstOrDefault(l => l.AddressType == AddressType.PreviousAddress);
            if (previousAddress != null)
            {
                udfList.AddRange(new UDF[]
                {
                    new UDF()
                    {
                        Name = AspireUdfFields.PreviousAddress,
                        Value = previousAddress.Street
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.PreviousAddressCity,
                        Value = previousAddress.City
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.PreviousAddressPostalCode,
                        Value = previousAddress.PostalCode
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.PreviousAddressState,
                        Value = previousAddress.State.ToProvinceCode()
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.PreviousAddressCountry,
                        Value = AspireUdfFields.DefaultAddressCountry
                    },
                });
            }

            var installationAddress = customer.Locations?.FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress);
            if (installationAddress != null)
            {
                udfList.AddRange(new UDF[]
                {
                    new UDF()
                    {
                        Name = AspireUdfFields.InstallationAddress,
                        Value = installationAddress.Street
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.InstallationAddressCity,
                        Value = installationAddress.City
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.InstallationAddressPostalCode,
                        Value = installationAddress.PostalCode
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.InstallationAddressState,
                        Value = installationAddress.State.ToProvinceCode()
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.InstallationAddressCountry,
                        Value = AspireUdfFields.DefaultAddressCountry
                    },
                });

                if (installationAddress.MoveInDate.HasValue)
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.EstimatedMoveInDate,
                        Value = installationAddress.MoveInDate.Value.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    });
                }
            }

            var mailingAddress = customer.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MailAddress);
            if (mailingAddress != null)
            {
                udfList.AddRange(new UDF[]
                {
                    new UDF()
                    {
                        Name = AspireUdfFields.MailingAddress,
                        Value = mailingAddress.Street
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.MailingAddressCity,
                        Value = mailingAddress.City
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.MailingAddressPostalCode,
                        Value = mailingAddress.PostalCode
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.MailingAddressState,
                        Value = mailingAddress.State.ToProvinceCode()
                    },
                    new UDF()
                    {
                        Name = AspireUdfFields.MailingAddressCountry,
                        Value = AspireUdfFields.DefaultAddressCountry
                    },
                });
            }

            if (isHomeOwner.HasValue)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.HomeOwner,
                    Value = isHomeOwner == true ? "Y" : "N"
                });
            }

            customer.Phones?.ForEach(p =>
            {
                switch (p.PhoneType)
                {
                    case PhoneType.Home:
                        udfList.Add(new UDF()
                        {
                            Name = AspireUdfFields.HomePhoneNumber,
                            Value = p.PhoneNum
                        });
                        break;
                    case PhoneType.Cell:
                        udfList.Add(new UDF()
                        {
                            Name = AspireUdfFields.MobilePhoneNumber,
                            Value = p.PhoneNum
                        });
                        break;
                    case PhoneType.Business:
                        udfList.Add(new UDF()
                        {
                            Name = AspireUdfFields.BusinessPhoneNumber,
                            Value = p.PhoneNum
                        });
                        break;                    
                }
            });

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.AllowCommunicate, Value = customer.AllowCommunicate.HasValue && customer.AllowCommunicate.Value ? "1" : "0"
            });

            if (customer.PreferredContactMethod.HasValue)
            {
                string contactMethod = null;
                switch (customer.PreferredContactMethod)
                {
                    case PreferredContactMethod.Email:
                        contactMethod = AspireUdfFields.ContactViaEmail;
                        break;
                    case PreferredContactMethod.Phone:
                        contactMethod = AspireUdfFields.ContactViaPhone;
                        break;
                    case PreferredContactMethod.Text:
                        contactMethod = AspireUdfFields.ContactViaText;
                        break;
                }
                if (contactMethod != null)
                {
                    udfList.Add(new UDF()
                    {
                        Name = contactMethod, Value = "Y"
                    });
                }
            }            

            return udfList;
        }

        #endregion
    }
}
