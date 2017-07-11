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
using System.Threading.Tasks;
using System.Web.Mvc;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;

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
            var contractResult = await _contractServiceAgent.GetContract(contractId);

            if (contractResult.Item1 != null && contractResult.Item2.All(c => c.Type != AlertType.Error))
            {
                var isNewlyCreated = contractResult.Item1.IsNewlyCreated;

                if (contractResult.Item1.IsNewlyCreated == true)
                {
                    var result = await _contractServiceAgent.NotifyContractEdit(contractId);

                    if (result.All(c => c.Type != AlertType.Error))
                    {
                        isNewlyCreated = false;
                    }
                }

                if (contractResult.Item1.ContractState == ContractState.CreditConfirmed && isNewlyCreated != true && contractResult.Item1.IsCreatedByCustomer != true)
                {
                    return RedirectToAction("EquipmentInformation", new { contractId });
                }

                if (contractResult.Item1.ContractState >= ContractState.Completed || contractResult.Item1.ContractState == ContractState.CreditCheckDeclined)
                {
                    return RedirectToAction("ContractEdit", "MyDeals", new { id = contractId });
                }
            }
            else
            {
                TempData[PortalConstants.CurrentAlerts] = contractResult?.Item2;
                return RedirectToAction("Error", "Info");
            }
            return RedirectToAction("BasicInfo", new { contractId = contractId});
        }


        public async Task<ActionResult> BasicInfo(int? contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if (contractId == null)
            {
                var contract = await _contractServiceAgent.CreateContract();
                if (contract?.Item1 != null)
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
            viewModel.ProvinceTaxRates = ( await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;
            if (viewModel?.ContractState >= ContractState.Completed)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = "Cannot edit applicants information for submitted contracts",
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

            if (!ModelState.IsValid)
            {
                basicInfo.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;

                return View(basicInfo);
            }

            Tuple<ContractDTO, IList<Alert>> result = basicInfo.ContractId == null ? 
                await _contractServiceAgent.CreateContract() : 
                await _contractServiceAgent.GetContract(basicInfo.ContractId.Value);

            if (result.Item1 == null)
            {
                return RedirectToAction("Error", "Info");
            }

            var updateResult = await _contractManager.UpdateContractAsync(basicInfo);

            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                TempData[PortalConstants.CurrentAlerts] = updateResult;

                return RedirectToAction("Error", "Info");
            }

            //Initiate a credit check here!
            await _contractServiceAgent.InitiateCreditCheck(result.Item1.Id);

            return RedirectToAction("CreditCheck", new { contractId = result.Item1.Id });
        }

        public async Task<ActionResult> CreditCheckConfirmation(int contractId)
        {
            var viewModel = await _contractManager.GetBasicInfoAsync(contractId);
            if (viewModel?.ContractState >= ContractState.Completed)
            {
                var alerts = new List<Alert>()
                        {
                            new Alert()
                            {
                                Type = AlertType.Error,
                                Message = "Cannot edit applicants information for submitted contracts",
                                Header = "Cannot edit contract"
                            }
                        };
                TempData[PortalConstants.CurrentAlerts] = alerts;
                return RedirectToAction("Error", "Info");
            }
            return View(viewModel);
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> CreditCheckConfirmation(BasicInfoViewModel basicInfo)
        //{
        //    ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
        //    if (!ModelState.IsValid)
        //    {
        //        return View(basicInfo);
        //    }
        //    //Initiate a credit check here!
        //    var initCheckResult = await _contractServiceAgent.InitiateCreditCheck(basicInfo.ContractId.Value);

        //    return RedirectToAction("CreditCheck", new { contractId = basicInfo.ContractId });
        //}

        public ActionResult CreditCheck(int contractId)
        {
            return View(contractId);
        }

        public async Task<ActionResult> CheckCreditStatus(int contractId)
        {
            //Initiate credit status check
            const int numOfAttempts = 2;
            TimeSpan timeOut = TimeSpan.FromSeconds(10);

            Tuple<CreditCheckDTO, IList<Alert>> checkResult = null;
            for (int i = 0; i < numOfAttempts; i++)
            {
                checkResult = await _contractServiceAgent.GetCreditCheckResult(contractId);
                if (checkResult == null || (checkResult.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
                {
                    break;
                }

                if (checkResult?.Item1 != null && checkResult.Item1.CreditCheckState != CreditCheckState.Initiated)
                {
                    break;
                }

                await Task.Delay(timeOut);
            }

            if ((checkResult?.Item2?.Any(a => a.Type == AlertType.Error && (a.Code == ErrorCodes.AspireConnectionFailed || a.Code == ErrorCodes.AspireTransactionNotCreated)) ?? false))
            {
                TempData["CreditCheckErrorMessage"] = Resources.Resources.CreditCheckErrorMessage;

                return RedirectToAction("BasicInfo", new { contractId });
            }

            if (checkResult?.Item1 == null && (checkResult?.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                TempData[PortalConstants.CurrentAlerts] = checkResult.Item2;
                return RedirectToAction("Error", "Info");
            }

            switch (checkResult?.Item1.CreditCheckState)
            {                
                case CreditCheckState.Approved:
                case CreditCheckState.MoreInfoRequired:
                    TempData["MaxCreditAmount"] = checkResult?.Item1.CreditAmount;
                    return RedirectToAction("EquipmentInformation", new { contractId });
                    break;
                case CreditCheckState.Declined:
                    return View("CreditDeclined", contractId);
                    break;                                
                case CreditCheckState.NotInitiated:
                case CreditCheckState.Initiated:
                default:
                    return View("CreditDeclined", contractId);
            }            
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
            for (int i = 0; i < numOfAttempts; i++)
            {
                checkResult = await _contractServiceAgent.GetCreditCheckResult(contractId);
                if (checkResult == null || (checkResult.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
                {
                    break;
                }
                if (checkResult?.Item1 != null && checkResult.Item1.CreditCheckState != CreditCheckState.Initiated)
                {
                    break;
                }
                await Task.Delay(timeOut);
            }

            if ((checkResult?.Item2?.Any(a => a.Type == AlertType.Error && (a.Code == ErrorCodes.AspireConnectionFailed || a.Code == ErrorCodes.AspireTransactionNotCreated)) ?? false))
            {
                TempData["CreditCheckErrorMessage"] = Resources.Resources.CreditCheckErrorMessage;
                var redirectStr = Url.Action("BasicInfo", new { contractId });
                return Json(redirectStr);
            }

            if (checkResult?.Item1 == null && (checkResult?.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                TempData[PortalConstants.CurrentAlerts] = checkResult.Item2;
                var redirectStr = Url.Action("Error", "Info");
                return Json(redirectStr);
            }

            switch (checkResult?.Item1.CreditCheckState)
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
            var model = await _contractManager.GetEquipmentInfoAsyncNew(contractId);

            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList();
            ViewBag.CardTypes = model.DealerTier?.RateCards?.Select(x => x.CardType).Distinct().ToList();
            ViewBag.AmortizationTerm = model.DealerTier?.RateCards?.ConvertToAmortizationSelectList();
            ViewBag.DefferalPeriod = model.DealerTier?.RateCards?.ConvertToDeferralSelectList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModelNew equipmentInfo)
        {
            ViewBag.IsAllInfoCompleted = false;

            var updateResult = await _contractManager.UpdateContractAsyncNew(equipmentInfo);

            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                TempData[PortalConstants.CurrentAlerts] = updateResult;

                return RedirectToAction("Error", "Info");
            }

            return RedirectToAction("ContactAndPaymentInfo", new { contractId = equipmentInfo.ContractId });
        }

        public async Task<ActionResult> AdditionalEquipmentInformation(int contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            return View(await _contractManager.GetAdditionalContactInfoAsyncNew(contractId));
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdditionalEquipmentInformation(ContactAndPaymentInfoViewModelNew contactAndPaymentInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if (!ModelState.IsValid)
            {
                return View();
            }

            var updateResult = await _contractManager.UpdateContractAsyncNew(contactAndPaymentInfo);

            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                TempData[PortalConstants.CurrentAlerts] = updateResult;

                return RedirectToAction("Error", "Info");
            }

            return RedirectToAction("ContactAndPaymentInfo", new { contractId = contactAndPaymentInfo.ContractId });
        }

        [HttpGet]
        public async Task<ActionResult> ContactAndPaymentInfo(int contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            return View(await _contractManager.GetContactAndPaymentInfoAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            if (!ModelState.IsValid)
            {
                return View();
            }

            var updateResult = await _contractManager.UpdateContractAsync(contactAndPaymentInfo);

            if (updateResult.Any(r => r.Type == AlertType.Error))
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

            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }
        
        public async Task<ActionResult> SubmitDeal(int contractId)
        {
            var result = await _contractServiceAgent.SubmitContract(contractId);

            if (result?.Item1?.CreditCheckState == CreditCheckState.Declined)
            {
                return RedirectToAction("CreditDeclined", new { contractId });
            }
            return RedirectToAction("AgreementSubmitSuccess", new { contractId });            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task SendContractEmails(SendEmailsViewModel emails)
        {
            if (!ModelState.IsValid)
            {
                return;
            }
            // update home owner notification email
            if (!string.IsNullOrEmpty(emails.BorrowerEmail))
            {
                var contractRes = await _contractServiceAgent.GetContract(emails.ContractId);
                if (contractRes?.Item1 != null)
                {
                    var emls = contractRes.Item1.PrimaryCustomer?.Emails;
                    if (emls?.Any(e => e.EmailType == EmailType.Notification) ?? false)
                    {
                        emls.First(e => e.EmailType == EmailType.Notification).EmailAddress =
                            emails.BorrowerEmail;
                    }
                    else
                    {
                        if (emls == null)
                        {
                            emls = new List<EmailDTO>();
                        }
                        emls.Add(new EmailDTO()
                        {
                            CustomerId = emails.HomeOwnerId,
                            EmailType = EmailType.Notification,
                            EmailAddress = emails.BorrowerEmail
                        });
                    }
                    var customer = new CustomerDataDTO()
                    {
                        Id = emails.HomeOwnerId,
                        ContractId = emails.ContractId,
                        Emails = emls
                    };

                    await _contractServiceAgent.UpdateCustomerData(new CustomerDataDTO[] { customer });
                }
            }
            SignatureUsersDTO signatureUsers = new SignatureUsersDTO();
            signatureUsers.ContractId = emails.ContractId;
            signatureUsers.Users = new List<SignatureUser>();
            signatureUsers.Users.Add(new SignatureUser()
            {
                EmailAddress = emails.BorrowerEmail, Role = SignatureRole.HomeOwner
            });

            emails.AdditionalApplicantsEmails?.ForEach(us =>
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = us.Email, Role = SignatureRole.AdditionalApplicant
                });
            });
            if (!string.IsNullOrEmpty(emails.SalesRepEmail))
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = emails.SalesRepEmail,
                    Role = SignatureRole.Dealer
                });
            }

            await _contractServiceAgent.InitiateDigitalSignature(signatureUsers);
        }

        public async Task<ActionResult> AgreementSubmitSuccess(int contractId)
        {
            var contract = await _contractServiceAgent.GetContract(contractId);
            if (contract.Item1 == null || (contract.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
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
            if (contract.Item1.SecondaryCustomers?.Any() ?? false)
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
            if (!string.IsNullOrEmpty(dealer?.Email))
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
            if (result.Item1.HasValue && result.Item2.All(a => a.Type != AlertType.Error))
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

            if (result.Item1 != null)
            {
                var response = new FileContentResult(result.Item1.DocumentRaw, "application/pdf")
                {
                    FileDownloadName = result.Item1.Name
                };
                if (!string.IsNullOrEmpty(response.FileDownloadName) && !response.FileDownloadName.ToLowerInvariant().EndsWith(".pdf"))
                {
                    response.FileDownloadName += ".pdf";
                }
                return response;
            }            

            return new FileContentResult(new byte[] {}, "application/pdf");
        }

        [HttpPost]
        public async Task<JsonResult> RecognizeDriverLicense(string imgBase64)
        {
            if (imgBase64 == null)
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
        public async Task<JsonResult> RecognizeDriverLicensePhoto()
        {
            if (Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }
            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() {ImageForReadRaw = data};
            var result = await _scanProcessingServiceAgent.ScanDriverLicense(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        [HttpPost]
        public async Task<JsonResult> RecognizeVoidCheque(string imgBase64)
        {
            if (imgBase64 == null)
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
            if (Request.Files == null || Request.Files.Count <= 0)
            {
                return GetErrorJson();
            }
            var file = Request.Files[0];
            byte[] data = new byte[file.ContentLength];
            file.InputStream.Read(data, 0, file.ContentLength);
            ScanningRequest scanningRequest = new ScanningRequest() {ImageForReadRaw = data};
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