using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Enumeration.Dealer;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Aspire.Integration.Constants;
using DealnetPortal.Aspire.Integration.Models;
using DealnetPortal.Aspire.Integration.ServiceAgents;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Dealer;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
using Microsoft.Practices.ObjectBuilder2;
using Address = DealnetPortal.Aspire.Integration.Models.Address;
using Application = DealnetPortal.Aspire.Integration.Models.Application;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireService : IAspireService
    {
        private readonly IAspireServiceAgent _aspireServiceAgent;
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly IDealerOnboardingRepository _dealerOnboardingRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppConfiguration _configuration;
        private readonly TimeSpan _aspireRequestTimeout;

        //Aspire codes
        private const string CodeSuccess = "T000";
        //symbols excluded from document names for upload
        private string DocumentNameReplacedSymbols = " -+=#@$%^!~&;:'`(){}.,|\"";
        private const string BlankValue = "-";

        public AspireService(IAspireServiceAgent aspireServiceAgent, IContractRepository contractRepository, 
            IDealerOnboardingRepository dealerOnboardingRepository,
            IUnitOfWork unitOfWork, IAspireStorageReader aspireStorageReader, ILoggingService loggingService, IAppConfiguration configuration)
        {
            _aspireServiceAgent = aspireServiceAgent;
            _aspireStorageReader = aspireStorageReader;
            _contractRepository = contractRepository;
            _dealerOnboardingRepository = dealerOnboardingRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _aspireRequestTimeout = TimeSpan.FromSeconds(90);
        }

        public async Task<IList<Alert>> UpdateContractCustomer(int contractId, string contractOwnerId, string leadSource = null)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                var result = await UpdateContractCustomer(contract, contractOwnerId, leadSource);
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

        public async Task<IList<Alert>> UpdateContractCustomer(Contract contract, string contractOwnerId, string leadSource = null)
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
                        Accounts = GetCustomersInfo(contract, leadSource)
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

        public async Task<IList<Alert>> SubmitDeal(int contractId, string contractOwnerId, string leadSource = null)
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
                        var application = new Application();
                        // Code change to calculate down payment and admin fee on first equipment amount
                        if (i == 0 && contract.Equipment.AgreementType == AgreementType.LoanApplication)
                        {
                            application = GetContractApplication(contract, eqToUpdate, 0, leadSource);
                        }
                        else
                        {
                            application = GetContractApplication(contract, eqToUpdate, 1, leadSource);
                        }
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
                                Application = application
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


        public async Task<IList<Alert>> SendDealUDFs(int contractId, string contractOwnerId, string leadSource = null)
        {
            var alerts = new List<Alert>();

            var contract = _contractRepository.GetContract(contractId, contractOwnerId);

            if (contract != null)
            {
                var result = await SendDealUDFs(contract, contractOwnerId, leadSource);
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

        public async Task<IList<Alert>> SendDealUDFs(Contract contract, string contractOwnerId, string leadSource = null, ContractorDTO contractor = null)
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
                        Application = GetSimpleContractApplication(contract, leadSource, contractor)
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
                            
                            Status = _configuration.GetSetting(WebConfigKeys.DOCUMENT_UPLOAD_STATUS_CONFIG_KEY)
                        };

                        var uploadName = Regex.Replace(Path.GetFileNameWithoutExtension(document.DocumentName).Replace('[', '_').Replace(']', '_'),
                            $"[{DocumentNameReplacedSymbols}]", "_");

                        var extn = "";
                        if (!String.IsNullOrWhiteSpace(Path.GetExtension(document.DocumentName)))
                        {
                            extn = Path.GetExtension(document.DocumentName)?.Substring(1);
                        }
                        request.Payload.Documents = new List<Document>()
                        {
                            new Document()
                            {
                                Name = uploadName,//Path.GetFileNameWithoutExtension(document.DocumentName), 
                                Data = Convert.ToBase64String(document.DocumentBytes),
                                Ext = extn
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

        public async Task<IList<Alert>> UploadDocument(string aspireTransactionId, ContractDocumentDTO document,
            string contractOwnerId)
        {
            if (aspireTransactionId == null)
            {
                throw new ArgumentNullException(nameof(aspireTransactionId));
            }
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (document.DocumentBytes == null)
            {
                throw new ArgumentNullException(nameof(document.DocumentBytes));
            }

            var alerts = new List<Alert>();

            if (!string.IsNullOrEmpty(aspireTransactionId) && document.DocumentBytes != null)
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
                            TransactionId = aspireTransactionId,
                            Status = _configuration.GetSetting(WebConfigKeys.DOCUMENT_UPLOAD_STATUS_CONFIG_KEY)
                        };

                        var uploadName = Regex.Replace(Path.GetFileNameWithoutExtension(document.DocumentName).Replace('[', '_').Replace(']', '_'),
                            $"[{DocumentNameReplacedSymbols}]", "_");

                        request.Payload.Documents = new List<Document>()
                        {
                            new Document()
                            {
                                Name = uploadName,//Path.GetFileNameWithoutExtension(document.DocumentName),
                                Data = Convert.ToBase64String(document.DocumentBytes),
                                Ext = Path.GetExtension(document.DocumentName)?.Substring(1)
                            }
                        };

                        var docUploadResponse = await _aspireServiceAgent.DocumentUploadSubmission(request).ConfigureAwait(false);
                        if (docUploadResponse?.Header == null || docUploadResponse.Header.Code != CodeSuccess || !string.IsNullOrEmpty(docUploadResponse.Header.ErrorMsg))
                        {
                            alerts.Add(new Alert()
                            {
                                Header = docUploadResponse?.Header?.Status,
                                Message = docUploadResponse?.Header?.Message ?? docUploadResponse?.Header?.ErrorMsg,
                                Type = AlertType.Error
                            });
                        }                       

                        if (alerts.All(a => a.Type != AlertType.Error))
                        {
                            _loggingService.LogInfo($"Document {document.DocumentName} to Aspire for contract with transactionId {aspireTransactionId} was uploaded successfully");
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
                        _loggingService.LogError($"Can't upload document to Aspire for transaction {aspireTransactionId}", ex);
                    }
                }
            }            

            return alerts;
        }

        public async Task<IList<Alert>> UploadOnboardingDocument(int dealerInfoId, int requiredDocId, string statusToSend = null)
        {
            var alerts = new List<Alert>();

            var dealerInfo = _dealerOnboardingRepository.GetDealerInfoById(dealerInfoId);
            var document = dealerInfo?.RequiredDocuments.First(d => d.Id == requiredDocId);
            if (dealerInfo != null && document != null && document.Uploaded != true)
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
                var userResult = GetAspireUser(dealerInfo.ParentSalesRepId);
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
                            TransactionId = string.IsNullOrEmpty(dealerInfo.TransactionId) ? null : dealerInfo.TransactionId,
                            Status = statusToSend ?? _configuration.GetSetting(WebConfigKeys.ONBOARDING_INIT_STATUS_KEY)
                        };

                        var uploadName = Regex.Replace(Path.GetFileNameWithoutExtension(document.DocumentName).Replace('[', '_').Replace(']', '_'),
                            $"[{DocumentNameReplacedSymbols}]", "_");

                        request.Payload.Documents = new List<Document>()
                        {
                            new Document()
                            {
                                Name = uploadName,//Path.GetFileNameWithoutExtension(document.DocumentName),
                                Data = Convert.ToBase64String(document.DocumentBytes),
                                Ext = Path.GetExtension(document.DocumentName)?.Substring(1)
                            }
                        };

                        var docUploadResponse = await _aspireServiceAgent.DocumentUploadSubmission(request).ConfigureAwait(false);
                        var rAlerts = AnalyzeDealerUploadResponse(docUploadResponse, dealerInfo);
                        if (rAlerts?.Any() == true)
                        {
                            alerts.AddRange(rAlerts);
                        }                        
                        if (alerts.All(a => a.Type != AlertType.Error))
                        {
                            document.Uploaded = true;
                            document.UploadDate = DateTime.Now;
                            _unitOfWork.Save();
                            _loggingService.LogInfo($"Document {document.DocumentName} for dealer onboarding form {dealerInfoId} was uploaded to Aspire successfully");
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
                        _loggingService.LogError($"Can't upload document to Aspire for dealer onboarding form {dealerInfoId}", ex);
                    }
                }                
            }
            else
            {
                if (document?.Uploaded == true)
                {
                    alerts.Add(new Alert()
                    {                        
                        Header = "Document was uploaded already",
                        Message = "Document was uploaded already",
                        Type = AlertType.Warning
                    });
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Code = ErrorCodes.CantGetContractFromDb,
                        Header = "Can't get dealer onboadring form",
                        Message = $"Can't get dealer onboadring form id {dealerInfoId}",
                        Type = AlertType.Error
                    });
                    _loggingService.LogError($"Can't get dealer onboadring form id {dealerInfoId}");
                }                
            }

            return alerts;
        }

        public async Task<string> GetDealStatus(string aspireTransactionId)
        {
            var status = await Task.Run(() => _aspireStorageReader.GetDealStatus(aspireTransactionId));
            return status;
        }

        public async Task<Tuple<string, IList<Alert>>> ChangeDealStatusEx(string aspireTransactionId, string newStatus,
            string contractOwnerId)
        {
            var tryChangeByCreditReview =
                await ChangeDealStatusByCreditReview(aspireTransactionId, newStatus, contractOwnerId);
            if (tryChangeByCreditReview?.Item2?.Any(a => a.Type == AlertType.Error) == true)
            {
                //sometimes we got an error with Credit Review
                var tryChangeByDocUpload = await ChangeDealStatus(aspireTransactionId, newStatus, contractOwnerId);
                string status = tryChangeByDocUpload?.All(e => e.Type != AlertType.Error) == true ? newStatus : null;
                return new Tuple<string, IList<Alert>>(status, tryChangeByDocUpload);
            }
            else
            {
                return tryChangeByCreditReview;
            }
        }

        public async Task<IList<Alert>> ChangeDealStatus(string aspireTransactionId, string newStatus, string contractOwnerId, string additionalDataToPass = null)
        {
            var alerts = new List<Alert>();            

            var request = new DocumentUploadRequest();

            var userResult = GetAspireUser(contractOwnerId);
            if (userResult.Item2.Any())
            {
                alerts.AddRange(userResult.Item2);
            }
            if (string.IsNullOrEmpty(newStatus))
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Cannot change deal status",
                    Message = "newStatus cannot be null"
                });
            }
            if (alerts.All(a => a.Type != AlertType.Error))
            {
                request.Header = userResult.Item1;

                try
                {
                    request.Payload = new DocumentUploadPayload()
                    {
                        TransactionId = aspireTransactionId,
                        Status = newStatus
                    };
                    
                    var submitStrBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(additionalDataToPass ?? newStatus));

                    request.Payload.Documents = new List<Document>()
                    {
                        new Document()
                        {
                            Name = newStatus,
                            Data = submitStrBase64,
                            Ext = "txt"
                        }
                    };

                    var docUploadResponse = await _aspireServiceAgent.DocumentUploadSubmission(request).ConfigureAwait(false);

                    if(docUploadResponse?.Header == null || docUploadResponse.Header.Code != CodeSuccess || !string.IsNullOrEmpty(docUploadResponse.Header.ErrorMsg))
                    {
                        alerts.Add(new Alert()
                        {
                            Header = docUploadResponse?.Header?.Status,
                            Message = docUploadResponse?.Header?.Message ?? docUploadResponse?.Header?.ErrorMsg,
                            Type = AlertType.Error
                        });
                    }                    

                    if (alerts.All(a => a.Type != AlertType.Error))
                    {
                        _loggingService.LogInfo($"Aspire state for transaction {aspireTransactionId} was updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    alerts.Add(new Alert()
                    {
                        Code = ErrorCodes.AspireConnectionFailed,
                        Header = $"Can't update state for transaction {aspireTransactionId}",
                        Message = ex.ToString(),
                        Type = AlertType.Error
                    });
                    _loggingService.LogError($"Can't update state for transaction {aspireTransactionId}", ex);
                }
            }

            return alerts;
        }

        public async Task<Tuple<string, IList<Alert>>> ChangeDealStatusByCreditReview(string aspireTransactionId, string newStatus,
            string contractOwnerId)
        {
            var alerts = new List<Alert>();
            string contractStatus = null;
            var request = new CreditCheckRequest();

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
                    request.Payload = new Payload()
                    {
                        TransactionId = aspireTransactionId,
                        ContractStatus = newStatus
                    };
                    
                    var response = await _aspireServiceAgent.CreditCheckSubmission(request).ConfigureAwait(false);

                    if (response?.Header == null || response.Header.Code != CodeSuccess || !string.IsNullOrEmpty(response.Header.ErrorMsg))
                    {
                        alerts.Add(new Alert()
                        {
                            Header = response?.Header?.Status,
                            Message = response?.Header?.Message ?? response?.Header?.ErrorMsg,
                            Type = AlertType.Error
                        });
                    }
                    if (!string.IsNullOrEmpty(response?.Payload?.ContractStatus))
                    {
                        contractStatus = response.Payload.ContractStatus;
                    }

                    if (alerts.All(a => a.Type != AlertType.Error))
                    {
                        _loggingService.LogInfo($"Aspire state for transaction {aspireTransactionId} was updated successfully");
                    }
                }
                catch (Exception ex)
                {
                    alerts.Add(new Alert()
                    {
                        Code = ErrorCodes.AspireConnectionFailed,
                        Header = $"Can't update state for transaction {aspireTransactionId}",
                        Message = ex.ToString(),
                        Type = AlertType.Error
                    });
                    _loggingService.LogError($"Can't update state for transaction {aspireTransactionId}", ex);
                }
            }

            return new Tuple<string, IList<Alert>>(contractStatus, alerts);
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
                                Status = _configuration.GetSetting(WebConfigKeys.ALL_DOCUMENTS_UPLOAD_STATUS_CONFIG_KEY)
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

        public async Task<IList<Alert>> SubmitDealerOnboarding(int dealerInfoId, string leadSource = null)
        {
            var alerts = new List<Alert>();

            var dealerInfo = _dealerOnboardingRepository.GetDealerInfoById(dealerInfoId);

            if (dealerInfo != null)
            {                
                CustomerRequest request = new CustomerRequest();

                var userResult = GetAspireUser(dealerInfo.ParentSalesRepId);
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
                                TransactionId = dealerInfo.TransactionId
                            },
                            Accounts = GetDealerOnboardingAccounts(dealerInfo, leadSource)
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

                        var rAlerts = AnalyzeDealerUploadResponse(response, dealerInfo);
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
                    Header = "Can't get dealer onboarding info",
                    Message = $"Can't get dealer onboarding info with id {dealerInfoId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get dealer onboarding info with id {dealerInfoId}");
            }

            alerts.Where(a => a.Type == AlertType.Error).ForEach(a =>
                _loggingService.LogError($"Aspire issue: {a.Header} {a.Message}"));

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Dealer onboarding info [{dealerInfoId}] submitted to Aspire successfully with transaction Id [{dealerInfo?.TransactionId}]");
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
                    !string.IsNullOrEmpty(dealer.Secure_AspirePassword))
                {
                    header = new RequestHeader()
                    {
                        UserId = dealer.AspireLogin,
                        Password = dealer.Secure_AspirePassword
                    };
                }
                else
                {
                    header = new RequestHeader()
                    {
                        UserId = _configuration.GetSetting(WebConfigKeys.ASPIRE_USER_CONFIG_KEY),
                        Password = _configuration.GetSetting(WebConfigKeys.ASPIRE_PASSWORD_CONFIG_KEY)
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

        private List<Account> GetCustomersInfo(Domain.Contract contract, string leadSource = null)
        {
            const string CustRole = "CUST";
            const string GuarRole = "GUAR";

            var accounts = new List<Account>();
            var portalDescriber = _configuration.GetSetting($"PortalDescriber.{contract.Dealer?.ApplicationId}");

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
                            Abbrev = AspireUdfFields.DefaultAddressCountry
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

                string setLeadSource = !string.IsNullOrEmpty(leadSource) ? leadSource :
                                    (_configuration.GetSetting(WebConfigKeys.DEFAULT_LEAD_SOURCE_KEY) ??
                                    (_configuration.GetSetting($"PortalDescriber.{contract.Dealer?.ApplicationId}") ?? contract.Dealer?.Application?.LeadSource));

                if (string.IsNullOrEmpty(c.AccountId))
                {
                    //check user on Aspire
                    var postalCode =
                        c.Locations?.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.PostalCode ??
                        c.Locations?.FirstOrDefault()?.PostalCode;
                    try
                    {
                        var aspireCustomer = _aspireStorageReader.FindCustomer(c.FirstName, c.LastName, c.DateOfBirth,
                            postalCode);//AutoMapper.Mapper.Map<CustomerDTO>();
                        if (!string.IsNullOrEmpty(aspireCustomer?.EntityId))
                        {
                            account.ClientId = aspireCustomer.EntityId.Trim();
                            c.ExistingCustomer = true;
                            //check lead source in aspire
                            if (!string.IsNullOrEmpty(aspireCustomer.LeaseSource))
                            {
                                setLeadSource = null;
                            }
                        }
                        else
                        {
                            c.ExistingCustomer = false;
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
                    //check lead source in aspire
                    var aspireCustomer = _aspireStorageReader.GetCustomerById(c.AccountId);
                    if (!string.IsNullOrEmpty(aspireCustomer?.LeaseSource))
                    {
                        setLeadSource = null;
                    }
                } 
                
                account.UDFs = GetCustomerUdfs(c, location, setLeadSource, contract.HomeOwners?.Any(hw => hw.Id == c.Id)).ToList();                

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

        private List<Account> GetDealerOnboardingAccounts(DealerInfo dealerInfo, string leadSource = null)
        {            
            var accounts = new List<Account>();
            accounts.Add(GetCompanyAccount(dealerInfo));
            if (dealerInfo.Owners?.Any() == true)
            {
                var companyOwners = GetCompanyOwnersAccounts(dealerInfo, leadSource);
                if (companyOwners.Any())
                {
                    accounts.Add(companyOwners.First());
                }
            }
            return accounts;
        }

        private Account GetCompanyAccount(DealerInfo dealerInfo)
        {
            var companyInfo = dealerInfo.CompanyInfo;
            const string companyRole = "OTHER";
            var account = new Account
            {
                ClientId = companyInfo.AccountId,
                Role = companyRole,
                IsIndividual = false,
                IsPrimary = true,
                Legalname = companyInfo.FullLegalName,                
                //Dba = companyInfo.OperatingName,
                EmailAddress = companyInfo.EmailAddress,                
                CreditReleaseObtained = true,
                Address = new Address()
                {
                    City = companyInfo.CompanyAddress?.City,
                    Province = new Province()
                    {
                        Abbrev = companyInfo.CompanyAddress?.State.ToProvinceCode()
                    },
                    Postalcode = companyInfo.CompanyAddress?.PostalCode,
                    Country = new Country()
                    {
                        Abbrev = AspireUdfFields.DefaultAddressCountry
                    },
                    StreetName = companyInfo.CompanyAddress?.Street,
                    SuiteNo = companyInfo.CompanyAddress?.Unit,
                    StreetNo = string.Empty
                },
                Telecomm = new Telecomm()
                {
                    Phone = companyInfo.Phone,
                    Email = companyInfo.EmailAddress,
                    Website = companyInfo.Website
                },
                UDFs = GetCompanyUdfs(dealerInfo).ToList()
            };
            return account;
        }

        private IList<Account> GetCompanyOwnersAccounts(DealerInfo dealerInfo, string leadSource = null)
        {
            const string ownerRole = "CUST";
            var accounts = dealerInfo.Owners?.OrderBy(o => o.OwnerOrder).Select(owner =>
            {
                var account = new Account
                {
                    Role = ownerRole,
                    IsIndividual = true,
                    IsPrimary = true,                    
                    Legalname = $"{owner.FirstName} {owner.LastName}",
                    EmailAddress = owner.EmailAddress,
                    CreditReleaseObtained = true,

                    Personal = new Personal()
                    {
                        Firstname = owner.FirstName,
                        Lastname = owner.LastName,
                        Dob = owner.DateOfBirth?.ToString("d", CultureInfo.CreateSpecificCulture("en-US"))
                    },
                    Address = new Address()
                    {
                        City = owner.Address?.City,
                        Province = new Province()
                        {
                            Abbrev = owner.Address?.State.ToProvinceCode()
                        },
                        Postalcode = owner.Address?.PostalCode,
                        Country = new Country()
                        {
                            Abbrev = AspireUdfFields.DefaultAddressCountry
                        },
                        StreetName = owner.Address?.Street,
                        SuiteNo = owner.Address?.Unit,
                        StreetNo = string.Empty
                    },
                    Telecomm = new Telecomm()
                    {
                        Phone = owner.MobilePhone ?? owner.HomePhone,
                        Email = owner.EmailAddress
                    }
                };

                var setLeadSource = !string.IsNullOrEmpty(leadSource) ? leadSource : _configuration.GetSetting(WebConfigKeys.ONBOARDING_LEAD_SOURCE_KEY) 
                                                                                    ?? _configuration.GetSetting(WebConfigKeys.DEFAULT_LEAD_SOURCE_KEY);
                if (string.IsNullOrEmpty(owner.AccountId))
                {
                    //check user on Aspire
                    var postalCode = owner.Address?.PostalCode;
                    try
                    {
                        var aspireCustomer = _aspireStorageReader.FindCustomer(owner.FirstName, owner.LastName,
                            owner.DateOfBirth ?? new DateTime(), postalCode);
                        if (aspireCustomer != null)
                        {
                            account.ClientId = aspireCustomer.EntityId?.Trim();
                            //check lead source in aspire
                            if (!string.IsNullOrEmpty(aspireCustomer.LeaseSource))
                            {
                                setLeadSource = null;
                            }
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
                    account.ClientId = owner.AccountId;
                    //check lead source in aspire
                    var aspireCustomer = _aspireStorageReader.GetCustomerById(account.ClientId);
                    if (!string.IsNullOrEmpty(aspireCustomer?.LeaseSource))
                    {
                        setLeadSource = null;
                    }
                }

                var UDFs = new List<UDF>();
                UDFs.Add(new UDF()
                {
                    Name = AspireUdfFields.HomePhoneNumber,
                    Value = !string.IsNullOrEmpty(owner.HomePhone) ? owner.HomePhone : BlankValue
                });
                UDFs.Add(new UDF()
                {
                    Name = AspireUdfFields.MobilePhoneNumber,
                    Value = !string.IsNullOrEmpty(owner.MobilePhone) ? owner.MobilePhone : BlankValue
                });
                if (!string.IsNullOrEmpty(setLeadSource))
                {
                    UDFs.Add(new UDF()
                    {
                        Name = AspireUdfFields.CustomerLeadSource,
                        Value = setLeadSource
                    });
                }
                if (UDFs.Any())
                {
                    account.UDFs = UDFs;
                }
                return account;
            }).ToList();
            return accounts ?? new List<Account>();
        }

        private Application GetContractApplication(Domain.Contract contract, ICollection<NewEquipment> newEquipments = null, int equipmentcount = 1, string leadSource = null)
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
                        Cost = contract.Equipment.AgreementType == AgreementType.LoanApplication && eq.Cost.HasValue ? equipmentcount == 0 ? (eq.Cost + Math.Round(((decimal)(eq.Cost / 100 * (decimal)(pTaxRate.Rate))), 2) + (decimal)contract.Equipment.AdminFee - (decimal)contract.Equipment.DownPayment)?.ToString(CultureInfo.InvariantCulture) :
                                                                                                        (eq.Cost + Math.Round(((decimal)(eq.Cost / 100 * (decimal)(pTaxRate.Rate))), 2))?.ToString(CultureInfo.InvariantCulture)
                                                                                                    : eq.MonthlyCost?.ToString(CultureInfo.InvariantCulture),
                        Description = eq.Description,
                        AssetClass = new AssetClass() { AssetCode = eq.Type }
                    });
                });
                application.AmtRequested = contract.Equipment.ValueOfDeal.Value.ToString();
                application.TermRequested = contract.Equipment.AmortizationTerm?.ToString();
                application.Notes = contract.Details?.Notes ?? contract.Equipment.Notes;
                //TODO: Implement finance program selection
                application.FinanceProgram = contract.Dealer?.Application?.FinanceProgram;//"EcoHome Finance Program";

                application.ContractType = contract.Equipment?.AgreementType == AgreementType.LoanApplication
                    ? "LOAN"
                    : "RENTAL";
            }
            application.UDFs = GetApplicationUdfs(contract, leadSource).ToList();

            return application;
        }

        /// <summary>
        /// UDFs and some basic info only
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        private Application GetSimpleContractApplication(Domain.Contract contract, string leadSource = null, ContractorDTO contractor = null)
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
            application.UDFs = GetApplicationUdfs(contract, leadSource, contractor).ToList();

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

        private IList<Alert> AnalyzeDealerUploadResponse(DealUploadResponse response, DealerInfo dealerInfo)
        {
            var alerts = new List<Alert>();

            if (response?.Header == null || response.Header.Code != CodeSuccess ||
                !string.IsNullOrEmpty(response.Header.ErrorMsg))
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
                    if (!string.IsNullOrEmpty(response.Payload.TransactionId) && response.Payload.TransactionId != "0")
                    {                        
                        dealerInfo.TransactionId = response.Payload?.TransactionId;
                        _unitOfWork.Save();
                        _loggingService.LogInfo($"Aspire transaction Id [{response.Payload?.TransactionId}] created for dealer onboarding form");
                    }

                    if (!string.IsNullOrEmpty(response.Payload.ContractStatus) && dealerInfo.Status != response.Payload.ContractStatus)
                    {
                        dealerInfo.Status = response.Payload.ContractStatus;
                        _unitOfWork.Save();
                    }

                    if (response.Payload.Accounts?.Any() ?? false)
                    {
                        var idUpdated = false;
                        response.Payload.Accounts.ForEach(a =>
                        {
                            if (dealerInfo.CompanyInfo != null && a.Name.Contains(dealerInfo.CompanyInfo?.FullLegalName) && dealerInfo.CompanyInfo?.AccountId != a.Id)
                            {
                                dealerInfo.CompanyInfo.AccountId = a.Id;
                                idUpdated = true;
                            }

                            dealerInfo?.Owners.ForEach(c =>
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
                            _loggingService.LogInfo($"Aspire accounts created for {response.Payload.Accounts.Count} dealer onboarding");
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
                //if (scorePoints > 0 && scorePoints <= 180)
                //{
                //    checkResult.CreditAmount = 5000;
                //}
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

        private IList<UDF> GetApplicationUdfs(Domain.Contract contract, string leadSource = null, ContractorDTO contractor = null)
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
            var setLeadSource = !string.IsNullOrEmpty(leadSource)
                ? leadSource
                : (_configuration.GetSetting(WebConfigKeys.DEFAULT_LEAD_SOURCE_KEY) ??
                   (_configuration.GetSetting($"PortalDescriber.{contract.Dealer?.ApplicationId}") ??
                    contract.Dealer?.Application?.LeadSource));
            if (!string.IsNullOrEmpty(contract.Details.TransactionId))
            {
                //check lead source in aspire
                //setLeadSource = null;
            }
            if (!string.IsNullOrEmpty(setLeadSource))
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadSource,
                    Value = setLeadSource
                });
            }
            if (contractor != null)
            {
                if (!string.IsNullOrEmpty(contractor.CompanyName))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorName,
                        Value = contractor.CompanyName
                    });
                }
                if (!string.IsNullOrEmpty(contractor.City))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorCity,
                        Value = contractor.City
                    });
                }
                if (!string.IsNullOrEmpty(contractor.EmailAddress))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorEmail,
                        Value = contractor.EmailAddress
                    });
                }
                if (!string.IsNullOrEmpty(contractor.PhoneNumber))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorPhone,
                        Value = contractor.PhoneNumber
                    });
                }
                if (!string.IsNullOrEmpty(contractor.PostalCode))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorPostalCode,
                        Value = contractor.PostalCode
                    });
                }
                if (!string.IsNullOrEmpty(contractor.State))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorProvince,
                        Value = contractor.State
                    });
                }
                if (!string.IsNullOrEmpty(contractor.Street))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorStreet,
                        Value = contractor.Street
                    });
                }
                if (!string.IsNullOrEmpty(contractor.Unit))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorUnit,
                        Value = contractor.Unit
                    });
                }
                if (!string.IsNullOrEmpty(contractor.Website))
                {
                    udfList.Add(new UDF()
                    {
                        Name = AspireUdfFields.ReqContractorWebsite,
                        Value = contractor.Website
                    });
                }
            }

            return udfList;
        }

        private IList<UDF> GetCustomerUdfs(Domain.Customer customer, Location mainLocation, string leadSource, bool? isHomeOwner = null)
        {
            var udfList = new List<UDF>();
            if (!string.IsNullOrEmpty(leadSource))
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.CustomerLeadSource,
                    Value = leadSource
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
                    Value = customer.ExistingCustomer == true || (!customer.ExistingCustomer.HasValue && !string.IsNullOrEmpty(customer.AccountId)) ? "Y" : "N"
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

            if (customer.AllowCommunicate.HasValue)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.AllowCommunicate,
                    Value = customer.AllowCommunicate.Value ? "1" : "0"
                });
            }

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

        private IList<UDF> GetCompanyUdfs(DealerInfo dealerInfo)
        {
            var udfList = new List<UDF>();

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.OperatingName,
                Value = !string.IsNullOrEmpty(dealerInfo?.CompanyInfo?.OperatingName) ? dealerInfo.CompanyInfo.OperatingName : BlankValue
            });
            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.NumberOfInstallers,
                Value = dealerInfo?.CompanyInfo?.NumberOfInstallers != null ? dealerInfo.CompanyInfo.NumberOfInstallers.GetEnumDescription()
                        : NumberOfPeople.Zero.GetEnumDescription()
            });
            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.NumberOfSalesPeople,
                Value = dealerInfo?.CompanyInfo?.NumberOfSales != null ? dealerInfo.CompanyInfo.NumberOfSales.GetEnumDescription()
                        : NumberOfPeople.Zero.GetEnumDescription()
            });
            if (dealerInfo?.CompanyInfo?.BusinessType != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.TypeOfBusiness,
                    Value = dealerInfo.CompanyInfo.BusinessType.GetEnumDescription()
                });
            }
            if (dealerInfo?.CompanyInfo?.YearsInBusiness != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.YearsInBusiness,
                    Value = dealerInfo.CompanyInfo.YearsInBusiness.GetEnumDescription()
                });
            }
            if (dealerInfo?.CompanyInfo?.Provinces?.Any() == true)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ProvincesApproved,
                    Value = GetCompanyProvincesApproved(dealerInfo)
                });
            }
            else
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ProvincesApproved,
                    Value = BlankValue
                });
            }

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.Website,
                Value = !string.IsNullOrEmpty(dealerInfo?.CompanyInfo?.Website) ? dealerInfo.CompanyInfo.Website : BlankValue
            });

            if (dealerInfo.ProductInfo?.Brands?.Any() == true || !string.IsNullOrEmpty(dealerInfo.ProductInfo?.PrimaryBrand))
            {
                var brandsList = new List<string>();
                if (!string.IsNullOrEmpty(dealerInfo.ProductInfo?.PrimaryBrand))
                {
                    brandsList.Add(dealerInfo.ProductInfo.PrimaryBrand);
                }
                if (dealerInfo.ProductInfo?.Brands?.Any() == true)
                {
                    brandsList.AddRange(dealerInfo.ProductInfo.Brands.Select(b => b.Brand));
                }
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ManufacturerBrandsSold,
                    Value = string.Join(", ", brandsList)
                });
            }
            else
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ManufacturerBrandsSold,
                    Value = BlankValue
                });
            }

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.AnnualSalesVolume,
                Value = dealerInfo.ProductInfo?.AnnualSalesVolume != null ? dealerInfo.ProductInfo.AnnualSalesVolume?.ToString(CultureInfo.InvariantCulture) : BlankValue
            });

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.AverageTransactionSize,
                Value = dealerInfo.ProductInfo?.AverageTransactionSize != null ? dealerInfo.ProductInfo.AverageTransactionSize?.ToString(CultureInfo.InvariantCulture) : BlankValue
            });

            if (dealerInfo.ProductInfo?.LeadGenReferrals != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadGeneratedWithReferrals,
                    Value = dealerInfo.ProductInfo.LeadGenReferrals == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.LeadGenLocalAdvertising != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadGeneratedWithLocalAdvertising,
                    Value = dealerInfo.ProductInfo.LeadGenLocalAdvertising == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.LeadGenTradeShows != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.LeadGeneratedWithTradeShows,
                    Value = dealerInfo.ProductInfo.LeadGenTradeShows == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.SalesApproachBroker != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ChannelTypeBroker,
                    Value = dealerInfo.ProductInfo.SalesApproachBroker == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.SalesApproachConsumerDirect != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ChannelTypeConsumerDirect,
                    Value = dealerInfo.ProductInfo.SalesApproachConsumerDirect == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.SalesApproachDistributor != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ChannelTypeDistributor,
                    Value = dealerInfo.ProductInfo.SalesApproachDistributor == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.SalesApproachDoorToDoor != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ChannelTypeDoorToDoorSales,
                    Value = dealerInfo.ProductInfo.SalesApproachDoorToDoor == true ? "Y" : "N"
                });
            }            
            if (dealerInfo.ProductInfo?.ProgramService != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ProgramServicesRequired,
                    Value = dealerInfo.ProductInfo.ProgramService == ProgramServices.Both ? "Financing + Leasing" : (dealerInfo.ProductInfo.ProgramService == ProgramServices.Loan ? "Leasing" : "Financing")
                });
            }
            if (dealerInfo.ProductInfo?.Relationship != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.RelationshipStructure,
                    Value = dealerInfo.ProductInfo.Relationship.GetEnumDescription()
                });
            }
            if (dealerInfo.ProductInfo?.WithCurrentProvider != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.CurrentFinanceProvider,
                    Value = dealerInfo.ProductInfo.WithCurrentProvider == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.OfferMonthlyDeferrals != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.OfferDeferrals,
                    Value = dealerInfo.ProductInfo.OfferMonthlyDeferrals == true ? "Y" : "N"
                });
            }
            if (dealerInfo.ProductInfo?.ReasonForInterest != null)
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ReasonForInterest,
                    Value = dealerInfo.ProductInfo.ReasonForInterest.GetEnumDescription()
                });
            }
            if (dealerInfo.ProductInfo?.Services?.Any() == true)
            {
                //sometimes equipment doesn't come with ProductInfo entity
                //var equipments = _contractRepository.GetEquipmentTypes();
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ProductsForFinancingProgram,
                    Value = string.Join(", ", dealerInfo.ProductInfo.Services.Select(s => s.Equipment?.Type))
                    //?? equipments.FirstOrDefault(eq => eq.Id == s.EquipmentId)?.Type
                });
            }
            else
            {
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.ProductsForFinancingProgram,
                    Value = BlankValue
                });
            }

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.OemName,
                Value = !string.IsNullOrEmpty(dealerInfo.ProductInfo?.OemName) ? dealerInfo.ProductInfo.OemName : BlankValue
            });

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.FinanceProviderName,
                Value = !string.IsNullOrEmpty(dealerInfo.ProductInfo?.FinanceProviderName) ? dealerInfo.ProductInfo.FinanceProviderName : BlankValue
            });

            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.MonthlyCapitalValue,
                Value = dealerInfo.ProductInfo?.MonthlyFinancedValue != null ? dealerInfo.ProductInfo.MonthlyFinancedValue?.ToString(CultureInfo.InvariantCulture) : BlankValue
            });
            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.MonthlyDealsToBeDeferred,
                Value = dealerInfo.ProductInfo?.PercentMonthlyDealsDeferred != null ? $"{dealerInfo.ProductInfo.PercentMonthlyDealsDeferred?.ToString(CultureInfo.InvariantCulture)}%" : $"{0.0M.ToString(CultureInfo.InvariantCulture)}%"
            });
            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.MarketingConsent,
                Value = dealerInfo.MarketingConsent ? "Y" : "N"
            });
            udfList.Add(new UDF()
            {
                Name = AspireUdfFields.CreditCheckConsent,
                Value = dealerInfo.CreditCheckConsent ? "Y" : "N"
            });

            if (dealerInfo?.Owners?.Any() == true)
            {
                udfList.AddRange(GetCompanyOwnersUdfs(dealerInfo));
            }

            if (!string.IsNullOrEmpty(dealerInfo.AccessKey))
            {
                var draftLink = _configuration.GetSetting(WebConfigKeys.DEALER_PORTAL_DRAFTURL_KEY) + dealerInfo.AccessKey;
                udfList.Add(new UDF()
                {
                    Name = AspireUdfFields.SubmissionUrl,
                    Value = draftLink
                });
            }

            //var leadSource = _configuration.GetSetting(WebConfigKeys.ONBOARDING_LEAD_SOURCE_KEY);
            //if (!string.IsNullOrEmpty(leadSource))
            //{
            //    udfList.Add(new UDF()
            //    {
            //        Name = AspireUdfFields.LeadSource,
            //        Value = leadSource
            //    });
            //}

            return udfList;
        }

        private string GetCompanyProvincesApproved(DealerInfo dealerInfo)
        {
            //probably will have more complex logic here, for include licences information
            //return string.Join(", ", dealerInfo.CompanyInfo.Provinces.Select(p => p.Province));
            var sb = new StringBuilder();
            var licenses = dealerInfo.AdditionalDocuments?.GroupBy(
                d => d.License?.LicenseDocuments?.FirstOrDefault()?.Province.Province);
            dealerInfo.CompanyInfo.Provinces.Select(p => p.Province).ForEach(p =>
            {
                var provLicenses = licenses?.FirstOrDefault(l => l.Key == p);
                if (provLicenses != null)
                {
                    var licInfo = string.Join("; ",
                        provLicenses.Select(
                            pl =>
                                $"License:{pl.License?.Name}, reg_number:{pl.Number}, expiry:{pl.ExpiredDate?.ToString("d", CultureInfo.CreateSpecificCulture("en-US")) ?? "no_expiry"}"));
                    sb.AppendLine($"{p}:{licInfo}");
                }
                else
                {
                    sb.AppendLine(p);
                }
            });
            return sb.ToString();
        }

        private IList<UDF> GetCompanyOwnersUdfs(DealerInfo dealerInfo)
        {
            const int maxOwners = 5;
            var ownerNum = 1;
            var owners = dealerInfo.Owners?.OrderBy(o => o.OwnerOrder).Take(maxOwners).ToList() ?? new List<OwnerInfo>();
            for (int i = owners.Count(); i < maxOwners; i++)
            {
                owners.Add(null);
            }
            
            var udfs = owners.SelectMany(owner =>
            {
                var ownerUdfs = new List<UDF>();
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerFirstName} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.FirstName) ? owner.FirstName : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerLastName} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.LastName) ? owner.LastName : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerDateOfBirth} {ownerNum}",
                    Value = owner?.DateOfBirth != null ? owner.DateOfBirth?.ToString("d", CultureInfo.CreateSpecificCulture("en-US")) : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerHomePhone} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.HomePhone) ? owner.HomePhone : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerMobilePhone} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.MobilePhone) ? owner.MobilePhone : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerEmail} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.EmailAddress) ? owner.EmailAddress : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerAddress} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.Address?.Street) ? owner.Address.Street : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerAddressCity} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.Address?.City) ? owner.Address.City : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerAddressPostalCode} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner?.Address?.PostalCode) ? owner.Address.PostalCode : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerAddressState} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner.Address.State) ? owner.Address.State : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerAddressUnit} {ownerNum}",
                    Value = !string.IsNullOrEmpty(owner.Address.Unit) ? owner.Address.Unit : BlankValue
                });
                ownerUdfs.Add(new UDF()
                {
                    Name = $"{AspireUdfFields.OwnerPercentageOfOwnership} {ownerNum}",
                    Value = owner?.PercentOwnership != null ? $"{owner.PercentOwnership?.ToString(CultureInfo.InvariantCulture)}%" : $"{0.0M.ToString(CultureInfo.InvariantCulture)}%"
                });               

                ownerNum++;
                return ownerUdfs;
            }).ToList();

            return udfs;
        }

        #endregion
    }
}
