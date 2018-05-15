using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;

using Microsoft.Practices.ObjectBuilder2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;
using ContractProvince = DealnetPortal.Web.Models.Enumeration.ContractProvince;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class NewRentalController : UpdateDataController
    {
        private readonly IScanProcessingServiceAgent _scanProcessingServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IContractManager _contractManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public NewRentalController(IScanProcessingServiceAgent scanProcessingServiceAgent, IContractServiceAgent contractServiceAgent,
            IDictionaryServiceAgent dictionaryServiceAgent, IContractManager contractManager) : base(contractManager)
        {
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
            _contractServiceAgent = contractServiceAgent;
            _contractManager = contractManager;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<ActionResult> ContractEdit(int contractId)
        {
            var contractResult = await _contractServiceAgent.GetContracts(new List<int> { contractId });
            var contract = contractResult.Item1.FirstOrDefault();
            if(contract != null && contractResult.Item2.All(c => c.Type != AlertType.Error))
            {
                var isNewlyCreated = contract.IsNewlyCreated;

                if(contract.IsNewlyCreated == true)
                {
                    var result = await _contractServiceAgent.NotifyContractEdit(contractId);

                    if(result.All(c => c.Type != AlertType.Error))
                    {
                        isNewlyCreated = false;
                    }
                }

                if(contract.ContractState == ContractState.CreditConfirmed && isNewlyCreated != true)
                {
                    return RedirectToAction("EquipmentInformation", new { contractId });
                }

                if(contract.ContractState >= ContractState.Completed || contract.ContractState == ContractState.CreditCheckDeclined)
                {
                    return RedirectToAction("ContractEdit", "MyDeals", new { id = contractId });
                }
            }
            else
            {
                TempData[PortalConstants.CurrentAlerts] = contractResult?.Item2;
                return RedirectToAction("Error", "Info");
            }
            return RedirectToAction("BasicInfo", new { contractId = contractId });
        }


        public async Task<ActionResult> BasicInfo(int? contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if(contractId == null)
            {
                var contract = await _contractServiceAgent.CreateContract();
                if(contract?.Item1 != null)
                {
                    contractId = contract.Item1.Id;
                }
                else
                {
                    TempData[PortalConstants.CurrentAlerts] = contract?.Item2;
                    return RedirectToAction("Error", "Info");
                }
            }

            var viewModel = await _contractManager.GetBasicInfoAsync(contractId.Value);
            viewModel.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;
            viewModel.VarificationIds = (await _dictionaryServiceAgent.GetAllVerificationIds()).Item1;

            var identity = (ClaimsIdentity)User.Identity;

            viewModel.QuebecDealer = identity.HasClaim(ClaimContstants.QuebecDealer, "True");

            if(viewModel?.ContractState >= ContractState.Closed)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = "Cannot edit applicants information for contracts sent to audit",
                                Header = "Cannot edit contract"
                            }
                        };
                TempData[PortalConstants.CurrentAlerts] = alerts;
                return RedirectToAction("Error", "Info");
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BasicInfo(BasicInfoViewModel basicInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if(!ModelState.IsValid)
            {
                basicInfo.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;
                basicInfo.VarificationIds = (await _dictionaryServiceAgent.GetAllVerificationIds()).Item1;

                return View(basicInfo);
            }

            Tuple<ContractDTO, IList<Alert>> result = !basicInfo.ContractId.HasValue ?
                await _contractServiceAgent.CreateContract() :
                await _contractServiceAgent.GetContract(basicInfo.ContractId.Value);

            if(result.Item1 == null)
            {
                return RedirectToAction("Error", "Info");
            }

            var updateResult = await _contractManager.UpdateContractAsync(basicInfo);

            if(updateResult.Any(r => r.Type == AlertType.Error))
            {
                TempData[PortalConstants.CurrentAlerts] = updateResult;

                return RedirectToAction("Error", "Info");
            }

            return RedirectToAction("CreditCheck", new { contractId = result.Item1.Id });
        }

        public ActionResult CreditCheck(int contractId)
        {
            return View(contractId);
        }

        public async Task<ActionResult> CreditDeclined(int contractId)
        {
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            var canAddApplicants = contractResult?.Item1 != null &&
                                   (contractResult.Item1.SecondaryCustomers == null ||
                                    contractResult.Item1.SecondaryCustomers.Count < PortalConstants.MaxAdditionalApplicants);
            var viewModel = new CreditRejectedViewModel()
            {
                ContractId = contractId,
                CanAddAdditionalApplicants = canAddApplicants
            };

            return View(viewModel);
        }

        public async Task<JsonResult> GetCreditCheckRedirection(int contractId)
        {
            //Initiate credit status check
            const int numOfAttempts = 2;
            TimeSpan timeOut = TimeSpan.FromSeconds(10);

            Tuple<CreditCheckDTO, IList<Alert>> checkResult = null;
            for(int i = 0; i < numOfAttempts; i++)
            {
                checkResult = await _contractServiceAgent.GetCreditCheckResult(contractId);
                if(checkResult == null || (checkResult.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
                {
                    break;
                }
                if(checkResult?.Item1 != null && checkResult.Item1.CreditCheckState != CreditCheckState.Initiated)
                {
                    break;
                }
                await Task.Delay(timeOut);
            }

            if((checkResult?.Item2?.Any(a => a.Type == AlertType.Error && (a.Code == ErrorCodes.AspireConnectionFailed || a.Code == ErrorCodes.AspireTransactionNotCreated)) ?? false))
            {
                TempData["CreditCheckErrorMessage"] = Resources.Resources.CreditCheckErrorMessage;
                var redirectStr = Url.Action("BasicInfo", new { contractId });
                return Json(redirectStr);
            }

            if(checkResult?.Item1 == null && (checkResult?.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                TempData[PortalConstants.CurrentAlerts] = checkResult.Item2;
                var redirectStr = Url.Action("Error", "Info");
                return Json(redirectStr);
            }

            switch(checkResult?.Item1.CreditCheckState)
            {
                case CreditCheckState.Approved:
                    TempData["MaxCreditAmount"] = checkResult?.Item1.CreditAmount;
                    var redirectStr = Url.Action("EquipmentInformation", new { contractId });
                    return Json(redirectStr);
                case CreditCheckState.MoreInfoRequired:
                    //TempData["MaxCreditAmount"] = checkResult?.Item1.CreditAmount;
                    redirectStr = Url.Action("EquipmentInformation", new { contractId });
                    return Json(redirectStr);
                case CreditCheckState.Declined:
                    redirectStr = Url.Action("CreditDeclined", new { contractId });
                    return Json(redirectStr);
                case CreditCheckState.NotInitiated:
                case CreditCheckState.Initiated:
                default:
                    redirectStr = Url.Action("CreditDeclined", new { contractId });
                    return Json(redirectStr);
            }
        }

        public ActionResult RateCard()
        {
            return View();
        }

        [HttpGet]
        public async Task<ActionResult> EquipmentInformation(int contractId)
        {
            var model = await _contractManager.GetEquipmentInfoAsync(contractId);
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
            ViewBag.CardTypes = model.DealerTier?.RateCards?.Select(x => x.CardType).Distinct().ToList();
            ViewBag.AmortizationTerm = model.DealerTier?.RateCards?.ConvertToAmortizationSelectList();
            ViewBag.DefferalPeriod = model.DealerTier?.RateCards?.ConvertToDeferralSelectList();
            if(model.DealerTier != null && model.DealerTier.Name == System.Configuration.ConfigurationManager.AppSettings[PortalConstants.ClarityTierNameKey])
            {
                ViewBag.totalAmountFinancedFor180amortTerm = 3999;
                ViewBag.LoanOnly = true;
            }
            else
            {
                ViewBag.totalAmountFinancedFor180amortTerm = 4999;
                ViewBag.LoanOnly = false;
            }
            if(model.DealProvince == ContractProvince.QC.ToString())
            {
                ViewBag.LoanOnly = true;
            }
            if(model.HomeOwner != null)
            {
                ViewBag.VarificationIds = (await _dictionaryServiceAgent.GetAllVerificationIds()).Item1;
            }
            ViewBag.AdminFee = 0;
	        var identity = (ClaimsIdentity) User.Identity;
            ViewBag.IsStandardRentalTier = identity.HasClaim(c => c.Type == ClaimNames.LeaseTier && string.IsNullOrEmpty(c.Value));
            ViewBag.IsQuebecDealer = identity.HasClaim(ClaimContstants.QuebecDealer, "True");
	        ViewBag.AgreementTypeAccessRights = identity.FindFirst(ClaimNames.AgreementType)?.Value.ToLower() ?? string.Empty;
            if(model.ContractState >= ContractState.Closed)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = $"Contract is not editable",
                                Header = "Cannot edit contract"
                            }
                        };
                TempData[PortalConstants.CurrentAlerts] = alerts;
                return RedirectToAction("Error", "Info");
            }

            return View("EquipmentInformation/EquipmentInformation", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModelNew equipmentInfo)
        {
            ViewBag.IsAllInfoCompleted = false;
            var ratecardValid = await _contractManager.CheckRateCard(equipmentInfo.ContractId.Value, equipmentInfo.SelectedRateCardId);

            if(ratecardValid)
            {
                var updateResult = await _contractManager.UpdateContractAsyncNew(equipmentInfo);

                if(updateResult.Any(r => r.Type == AlertType.Error))
                {
                    TempData[PortalConstants.CurrentAlerts] = updateResult;

                    return RedirectToAction("Error", "Info");
                }

                return RedirectToAction("ContactAndPaymentInfo", new { contractId = equipmentInfo.ContractId });
            }

            return RedirectToAction("EquipmentInformation", new { equipmentInfo.ContractId.Value });
        }

        [HttpGet]
        public async Task<ActionResult> ContactAndPaymentInfo(int contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            var contract = await _contractServiceAgent.GetContract(contractId);
            if(contract.Item1 == null || contract.Item1.ContractState >= ContractState.Closed)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = $"Contract is not editable",
                                Header = "Cannot edit contract"
                            }
                        };
                TempData[PortalConstants.CurrentAlerts] = alerts;
                return RedirectToAction("Error", "Info");
            }
            return View(await _contractManager.GetContactAndPaymentInfoAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if(!ModelState.IsValid)
            {
                return View();
            }

            var updateResult = await _contractManager.UpdateContractAsync(contactAndPaymentInfo);

            if(updateResult.Any(r => r.Type == AlertType.Error))
            {
                TempData[PortalConstants.CurrentAlerts] = updateResult;

                return RedirectToAction("Error", "Info");
            }

            return RedirectToAction("SummaryAndConfirmation", new { contractId = contactAndPaymentInfo.ContractId });
        }

        public async Task<ActionResult> SummaryAndConfirmation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
            ViewBag.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;
            var contract = await _contractServiceAgent.GetContract(contractId);
            if(contract.Item1 == null || contract.Item1.ContractState >= ContractState.Closed || contract.Item1.Equipment == null)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = $"Contract is not editable",
                                Header = "Cannot edit contract"
                            }
                        };
                TempData[PortalConstants.CurrentAlerts] = alerts;
                return RedirectToAction("Error", "Info");
            }
            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }

        public async Task<ActionResult> SubmitDeal(int contractId)
        {
            var contract = await _contractServiceAgent.GetContract(contractId);
            if(contract.Item1 == null || contract.Item1.ContractState >= ContractState.Closed)
            {
                return RedirectToAction("Error", "Info");
            }
            var rateCardValid = await _contractManager.CheckRateCard(contractId, null);
            if(rateCardValid)
            {
                var result = await _contractServiceAgent.SubmitContract(contractId);

                if(result?.Item1?.CreditCheckState == CreditCheckState.Declined)
                {
                    return RedirectToAction("CreditDeclined", new { contractId });
                }
                TempData[PortalConstants.IsNewlySubmitted] = true;
                return RedirectToAction("ContractEdit", "MyDeals", new { id = contractId });
            }
            return RedirectToAction("SummaryAndConfirmation", new { contractId });
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult> SendContractEmails(ESignatureViewModel eSignatureViewModel)
        {
            if(!ModelState.IsValid)
            {
                return GetErrorJson();
            }
            // update home owner notification email
            var borrowerSigner = eSignatureViewModel.Signers.SingleOrDefault(x => x.Role == SignatureRole.HomeOwner);
            if(!string.IsNullOrEmpty(borrowerSigner.Email))
            {
                var contractRes = await _contractServiceAgent.GetContract(eSignatureViewModel.ContractId);
                if(contractRes?.Item1 != null)
                {
                    var emls = contractRes.Item1.PrimaryCustomer?.Emails;
                    if(emls?.Any(e => e.EmailType == EmailType.Notification) ?? false)
                    {
                        emls.First(e => e.EmailType == EmailType.Notification).EmailAddress =
                            borrowerSigner.Email;
                    }
                    else
                    {
                        if(emls == null)
                        {
                            emls = new List<EmailDTO>();
                        }
                        emls.Add(new EmailDTO()
                        {
                            CustomerId = borrowerSigner.CustomerId.Value,
                            EmailType = EmailType.Notification,
                            EmailAddress = borrowerSigner.Email
                        });
                    }
                    var customer = new CustomerDataDTO()
                    {
                        Id = borrowerSigner.CustomerId.Value,
                        ContractId = eSignatureViewModel.ContractId,
                        Emails = emls
                    };

                    await _contractServiceAgent.UpdateCustomerData(new CustomerDataDTO[] { customer });
                }
            }
            SignatureUsersDTO signatureUsers = new SignatureUsersDTO();

            signatureUsers.ContractId = eSignatureViewModel.ContractId;
            signatureUsers.Users = new List<SignatureUser>();
            signatureUsers.Users.Add(new SignatureUser()
            {
                Id = borrowerSigner.Id,
                CustomerId = borrowerSigner.CustomerId,
                EmailAddress = borrowerSigner.Email,
                Role = SignatureRole.HomeOwner
            });

            eSignatureViewModel.Signers.Where(x => x.Role == SignatureRole.AdditionalApplicant).ForEach(us =>
              {
                  signatureUsers.Users.Add(new SignatureUser()
                  {
                      EmailAddress = us.Email,
                      Role = SignatureRole.AdditionalApplicant,
                      Id = us.Id,
                      CustomerId = us.CustomerId
                  });
              });
            var eSignatureSigner = eSignatureViewModel.Signers.SingleOrDefault(x => x.Role == SignatureRole.Dealer);
            if(!string.IsNullOrEmpty(borrowerSigner.Email))
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = eSignatureSigner.Email,
                    Role = SignatureRole.Dealer,
                    Id = eSignatureSigner.Id
                });
            }

            var result = await _contractServiceAgent.InitiateDigitalSignature(signatureUsers);

            if(result.Item2.Any(a => a.Type == AlertType.Error))
            {
                return Json(new { isError = true, message = result.Item2.FirstOrDefault(a => a.Type == AlertType.Error).Message });
            }

            return Json(new ESignatureViewModel
            {
                ContractId = result.Item1.ContractId,
                Status = result.Item1.Status,
                Signers = result.Item1.Signers.Select(s => new SignerViewModel
                {
                    CustomerId = s.CustomerId,
                    Comment = s.Comment,
                    Email = s.EmailAddress,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Id = s.Id,
                    Role = s.SignerType,
                    SignatureStatus = s.SignatureStatus,
                    StatusLastUpdateTime = s.StatusLastUpdateTime?.TryConvertToLocalUserDate()
                }).ToList()
            });
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateContractEmails(ESignatureViewModel eSignatureViewModel)
        {
            if(!ModelState.IsValid)
            {
                return GetErrorJson();
            }
            // update home owner notification email
            var borrowerSigner = eSignatureViewModel.Signers.SingleOrDefault(x => x.Role == SignatureRole.HomeOwner);
            if(borrowerSigner != null && !string.IsNullOrEmpty(borrowerSigner.Email))
            {
                var contractRes = await _contractServiceAgent.GetContract(eSignatureViewModel.ContractId);
                if(contractRes?.Item1 != null)
                {
                    var emls = contractRes.Item1.PrimaryCustomer?.Emails;
                    if(emls?.Any(e => e.EmailType == EmailType.Notification) ?? false)
                    {
                        emls.First(e => e.EmailType == EmailType.Notification).EmailAddress =
                            borrowerSigner.Email;
                    }
                    else
                    {
                        if(emls == null)
                        {
                            emls = new List<EmailDTO>();
                        }
                        emls.Add(new EmailDTO()
                        {
                            CustomerId = borrowerSigner.CustomerId.Value,
                            EmailType = EmailType.Notification,
                            EmailAddress = borrowerSigner.Email
                        });
                    }
                    var customer = new CustomerDataDTO()
                    {
                        Id = borrowerSigner.CustomerId.Value,
                        ContractId = eSignatureViewModel.ContractId,
                        Emails = emls
                    };

                    await _contractServiceAgent.UpdateCustomerData(new CustomerDataDTO[] { customer });
                }
            }
            SignatureUsersDTO signatureUsers = new SignatureUsersDTO();
            signatureUsers.ContractId = eSignatureViewModel.ContractId;
            signatureUsers.Users = new List<SignatureUser>();
            if(borrowerSigner != null)
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = borrowerSigner.Email,
                    Role = SignatureRole.HomeOwner,
                    Id = borrowerSigner.Id,
                    CustomerId = borrowerSigner.CustomerId
                });
            }
            eSignatureViewModel.Signers.Where(x => x.Role == SignatureRole.AdditionalApplicant).ForEach(us =>
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = us.Email,
                    Role = SignatureRole.AdditionalApplicant,
                    Id = us.Id,
                    CustomerId = us.CustomerId
                });
            });
            var eSignatureSigner = eSignatureViewModel.Signers.SingleOrDefault(x => x.Role == SignatureRole.Dealer);
            if(eSignatureSigner != null && !string.IsNullOrEmpty(eSignatureSigner.Email))
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = eSignatureSigner.Email,
                    Role = SignatureRole.Dealer,
                    Id = eSignatureSigner.Id
                });
            }

            var result = await _contractServiceAgent.UpdateContractSigners(signatureUsers);

            if(result.Any(a => a.Type == AlertType.Error))
            {
                return GetErrorJson();
            }
            return GetSuccessJson();
        }

        public async Task<ActionResult> AgreementSubmitSuccess(int contractId)
        {
            var contract = await _contractServiceAgent.GetContract(contractId);
            if(contract.Item1 == null || (contract.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                TempData[PortalConstants.CurrentAlerts] = contract.Item2;
                return RedirectToAction("Error", "Info");
            }

            var sendEmails = new SendEmailsViewModel();
            sendEmails.ContractId = contractId;
            sendEmails.HomeOwnerFullName = contract.Item1.PrimaryCustomer.FirstName + " " + contract.Item1.PrimaryCustomer.LastName;
            sendEmails.HomeOwnerId = contract.Item1.PrimaryCustomer.Id;
            sendEmails.BorrowerEmail = contract.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                contract.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress;
            sendEmails.SalesRep = contract.Item1.Equipment?.SalesRep;
            if(contract.Item1.SecondaryCustomers?.Any() ?? false)
            {
                sendEmails.AdditionalApplicantsEmails =
                    contract.Item1.SecondaryCustomers.Select(c =>
                        new CustomerEmail()
                        {
                            CustomerId = c.Id,
                            CustomerName = $"{c.FirstName} {c.LastName}",
                            Email = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                        c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress
                        }).ToArray();
            }
            var dealer = await _dictionaryServiceAgent.GetDealerInfo();
            if(!string.IsNullOrEmpty(dealer?.Email))
            {
                sendEmails.SalesRepEmail = dealer.Email;
            }
            sendEmails.AgreementType = contract.Item1.Equipment?.AgreementType.ConvertTo<AgreementType>() ?? AgreementType.LoanApplication;

            ViewBag.IsEsignatureEnabled = dealer?.EsignatureEnabled ?? false;

            return View(sendEmails);
        }

        public async Task<ActionResult> CreateNewApplication(int contractId)
        {
            var result = await _contractManager.CreateNewCustomerContract(contractId);
            if(result.Item1.HasValue && result.Item2.All(a => a.Type != AlertType.Error))
            {
                return RedirectToAction("BasicInfo", new { contractId = result.Item1 });
            }
            else
            {
                TempData[PortalConstants.CurrentAlerts] = result.Item2;
                return RedirectToAction("Error", "Info");
            }
        }

        public async Task<FileResult> PrintContract(int contractId)
        {
            var result = await _contractServiceAgent.GetContractAgreement(contractId);

            if(result.Item1 != null)
            {
                var response = new FileContentResult(result.Item1.DocumentRaw, "application/pdf")
                {
                    FileDownloadName = result.Item1.Name
                };
                if(!string.IsNullOrEmpty(response.FileDownloadName) && !response.FileDownloadName.ToLowerInvariant().EndsWith(".pdf"))
                {
                    response.FileDownloadName += ".pdf";
                }
                return response;
            }

            return new FileContentResult(new byte[] { }, "application/pdf");
        }

        public async Task<FileResult> GetSignedContract(int contractId)
        {
            var result = await _contractServiceAgent.GetSignedAgreement(contractId);

            if(result.Item1 != null)
            {
                var response = new FileContentResult(result.Item1.DocumentRaw, "application/pdf")
                {
                    FileDownloadName = result.Item1.Name
                };
                if(!string.IsNullOrEmpty(response.FileDownloadName) && !response.FileDownloadName.ToLowerInvariant().EndsWith(".pdf"))
                {
                    response.FileDownloadName += ".pdf";
                }
                return response;
            }

            return new FileContentResult(new byte[] { }, "application/pdf");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> RecognizeDriverLicense(string imgBase64)
        {
            if(imgBase64 == null)
            {
                return GetErrorJson();
            }
            imgBase64 = imgBase64.Replace("data:image/png;base64,", "");
            imgBase64 = imgBase64.Replace(' ', '+');
            var bytes = Convert.FromBase64String(imgBase64);
            var scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = bytes
            };
            var result = await _scanProcessingServiceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> RecognizeDriverLicensePhoto()
        {
            if(Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }
            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };
            var result = await _scanProcessingServiceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        [HttpPost]
        public async Task<JsonResult> RecognizeVoidCheque(string imgBase64)
        {
            if(imgBase64 == null)
            {
                return GetErrorJson();
            }
            imgBase64 = imgBase64.Replace("data:image/png;base64,", "");
            imgBase64 = imgBase64.Replace(' ', '+');
            var bytes = Convert.FromBase64String(imgBase64);
            var scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = bytes
            };
            var result = await _scanProcessingServiceAgent.ScanVoidCheque(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        [HttpPost]
        public async Task<JsonResult> RecognizeVoidChequePhoto()
        {
            if(Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }
            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };
            var result = await _scanProcessingServiceAgent.ScanVoidCheque(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        [HttpPost]
        public async Task<JsonResult> CheckContractAgreement(int contractId)
        {
            var result = await _contractServiceAgent.CheckContractAgreementAvailable(contractId);

            return Json(result.Item1);
        }

        [HttpPost]
        public async Task<ActionResult> ContractRemove(int contractId)
        {
            var result = await _contractServiceAgent.RemoveContract(contractId);
            return result.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}