using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Aspire;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Practices.ObjectBuilder2;
using Application = DealnetPortal.Api.Models.Aspire.Application;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireService : IAspireService
    {
        private readonly IAspireServiceAgent _aspireServiceAgent;
        private readonly IAspireStorageService _aspireStorageService;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;

        private readonly TimeSpan _aspireRequestTimeout;

        //Aspire codes
        private const string CodeSuccess = "T000";

        public AspireService(IAspireServiceAgent aspireServiceAgent, IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, IAspireStorageService aspireStorageService, ILoggingService loggingService)
        {
            _aspireServiceAgent = aspireServiceAgent;
            _aspireStorageService = aspireStorageService;
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
                            else
                            {
                                //TODO: Only for test. Remove in the future
                                creditCheckResult = new CreditCheckDTO()
                                {
                                    CreditCheckState = CreditCheckState.Approved,
                                    ScorecardPoints = 150,
                                    CreditAmount = 10000,
                                    ContractId = contractId
                                };
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
                            Application = GetContractApplication(contract)
                        }
                    };                   

                    try
                    {
                        Task timeoutTask = Task.Delay(_aspireRequestTimeout);
                        var aspireRequestTask = _aspireServiceAgent.DealUploadSubmission(request);
                        DealUploadResponse response = null;

                        if (await Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false) == aspireRequestTask)
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
                            //TODO: insert correct status
                            Status = "35-Dead Deal"
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
                    Header = ErrorConstants.AspireConnectionFailed,
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });
                _loggingService.LogError("Failed to communicate with Aspire", ex);
            }
            


            return alerts;
        }        

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
                        //From = new From()
                        //{
                        //    AccountNumber = dealer.AspireLogin,
                        //    Password = dealer.AspirePassword
                        //}
                    };
                }
                else
                {
                    header = new RequestHeader()
                    {
                        UserId = ConfigurationManager.AppSettings["AspireUser"],
                        Password = ConfigurationManager.AppSettings["AspirePassword"]
                        //From = new From()
                        //{
                        //    AccountNumber = ConfigurationManager.AppSettings["AspireUser"],
                        //    Password = ConfigurationManager.AppSettings["AspirePassword"]
                        //}
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

        private string GetTransactionId(Domain.Contract contract)
        {
            return contract?.Details?.TransactionId;

            //InitRequestPayload(request);
            //if (!string.IsNullOrEmpty(contract.Details?.TransactionId))
            //{
            //    if (request.Payload.Lease.Application == null)
            //    {
            //        request.Payload.Lease.Application = new Application();
            //    }
            //    request.Payload.Lease.Application.TransactionId = contract.Details.TransactionId;
            //}
        }

        private List<Account> GetCustomersInfo(Domain.Contract contract)
        {
            const string CustRole = "CUST";
            const string GuarRole = "GUAR";

            var accounts = new List<Account>();
            //InitRequestPayload(request);            

            Func<Domain.Customer, string, Account> fillAccount = (c, role) =>
            {
                var account = new Account
                {
                    IsIndividual = true,
                    IsPrimary = true,
                    EmailAddress = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                                   c.Emails?.FirstOrDefault()?.EmailAddress,
                    Personal = new Personal()
                    {
                        Firstname = c.FirstName,
                        Lastname = c.LastName,
                        Dob = c.DateOfBirth.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    },
                    UDFs = new List<UDF>()
                    {
                        new UDF()
                        {

                            Name = "Authorized Consent",
                            Value = "Y"
                        },
                        new UDF()
                        {

                            Name = "Existing Customer",
                            Value = "Y" // ???
                        }
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

                    account.UDFs.Add(new UDF()
                    {
                        Name = "Residence",
                        Value = loc.ResidenceType == ResidenceType.Own ? "O" : "R" //<!—other value is R for rent  and O for own-->
                    });
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
                //acc.IsPrimary = true;                
                accounts.Add(acc);
            }

            contract.SecondaryCustomers?.ForEach(c => accounts.Add(fillAccount(c, GuarRole)));
            return accounts;
        }

        private Application GetContractApplication(Domain.Contract contract)
        {
            var application = new Application()
            {
                TransactionId = contract.Details?.TransactionId
            };
            if (contract.Equipment != null)
            {
                contract.Equipment.NewEquipment?.ForEach(eq =>
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
                        Cost = eq.Cost?.ToString(),
                        Description = eq.Description,
                        AssetClass = new AssetClass() { AssetCode = eq.Type }
                    });
                });
                application.AmtRequested = contract.Equipment.AmortizationTerm?.ToString();
                application.TermRequested = contract.Equipment.RequestedTerm.ToString();
                application.Notes = contract.Equipment.Notes;
                //TODO: Implement finance program selection
                application.FinanceProgram = "EcoHome Finance Program";

                application.UDFs = new List<UDF>()
                {
                    new UDF()
                    {
                        Name = "Requested Term",
                        Value = contract.Equipment.RequestedTerm.ToString()
                    }
                };

            }
            if (contract.PaymentInfo != null)
            {
                var udfs = new List<UDF>();
                udfs.Add(new UDF()
                {
                    Name = "Contract Type",
                    Value = contract.Equipment?.AgreementType == AgreementType.LoanApplication ? "LOAN" : "RENTAL"
                });
                udfs.Add(new UDF()
                {
                    Name = "Payment Type",
                    Value = contract.PaymentInfo?.PaymentType == PaymentType.Enbridge ? "Enbridge" : "Pre-Authorized Debit"
                });
                //udfs.Add(new UDF()
                //{
                //    Name = "Payment Type",
                //    Value = contract.PaymentInfo?.PaymentType == PaymentType.Enbridge ? "Enbridge Bill" : "Pre-Authorized Debit"
                //});                
                if (contract.PaymentInfo?.PaymentType == PaymentType.Enbridge &&
                    !string.IsNullOrEmpty(contract.PaymentInfo?.EnbridgeGasDistributionAccount))
                {
                    udfs.Add(new UDF()
                    {
                        Name = "Enbridge Gas Account Number",
                        Value = contract.PaymentInfo.EnbridgeGasDistributionAccount
                    });
                }

                if (application.UDFs == null)
                {
                    application.UDFs = new List<UDF>();
                }
                application.UDFs.AddRange(udfs);
            }

            if (!string.IsNullOrEmpty(contract.ExternalSubDealerId))
            {
                var subDealers =
                    _aspireStorageService.GetSubDealersList(contract.Dealer.AspireLogin ?? contract.Dealer.UserName);
                var sbd = subDealers?.FirstOrDefault(sd => sd.SubmissionValue == contract.ExternalSubDealerId);
                if (sbd != null)
                {
                    application.UDFs.Add(new UDF()
                    {
                        Name = sbd.DealerName,
                        Value = sbd.SubmissionValue
                    });
                }
            }

            return application;
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

                    if (!string.IsNullOrEmpty(response.Payload.ContractStatus) && contract.Details != null &&
                        contract.Details.Status != response.Payload.ContractStatus)
                    {
                        contract.Details.Status = response.Payload.ContractStatus;
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
                        var aEq =
                            contract?.Equipment?.NewEquipment?.FirstOrDefault(
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
                if (scorePoints <= 150)
                {
                    checkResult.CreditAmount = 10000;
                }
                if (scorePoints > 150 && scorePoints <= 170)
                {
                    checkResult.CreditAmount = 12000;
                }
                if (scorePoints > 170 && scorePoints <= 180)
                {
                    checkResult.CreditAmount = 15000;
                }
                if (scorePoints > 180 && scorePoints <= 190)
                {
                    checkResult.CreditAmount = 20000;
                }
                if (scorePoints > 190)
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
    }
}
