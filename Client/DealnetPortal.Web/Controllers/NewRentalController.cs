using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.Controllers
{    
    [AuthFromContext]
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
                if (contractResult.Item1.ContractState == ContractState.CreditContirmed)
                {
                    return RedirectToAction("EquipmentInformation", new { contractId });
                }
                if (contractResult.Item1.ContractState == ContractState.Completed)
                {
                    return RedirectToAction("ContractEdit", "MyDeals", new { id = contractId });
                }
            }
            else
            {
                return RedirectToAction("Error", "Info", new { alers = contractResult?.Item2 });
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
                    return RedirectToAction("Error", "Info", new {alers = contract?.Item2});
                }
            }

            return View(await _contractManager.GetBasicInfoAsync(contractId.Value));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> BasicInfo(BasicInfoViewModel basicInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            if (!ModelState.IsValid)
            {
                return View();
            }
            var contractResult = basicInfo.ContractId == null ?
                await _contractServiceAgent.CreateContract() :
                await _contractServiceAgent.GetContract(basicInfo.ContractId.Value);
            if (contractResult?.Item1 != null)
            {
                var updateResult = await _contractManager.UpdateContractAsync(basicInfo);
                if (updateResult.Any(r => r.Type == AlertType.Error))
                {
                    return RedirectToAction("Error", "Info", new { alers = updateResult });
                }
            }
            return RedirectToAction("CreditCheckConfirmation", new { contractId = contractResult?.Item1?.Id ?? 0 });
        }

        public async Task<ActionResult> CreditCheckConfirmation(int contractId)
        {
            return View(await _contractManager.GetBasicInfoAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreditCheckConfirmation(BasicInfoViewModel basicInfo)
        {
            //Initiate a credit check here!
            var initCheckResult = await _contractServiceAgent.InitiateCreditCheck(basicInfo.ContractId.Value);

            return RedirectToAction("CreditCheck", new { contractId = basicInfo.ContractId });
        }

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
                TempData["CreditCheckErrorMessage"] = "Can't connect to Aspire for Credit Check. Try to perform Credit Check later.";
                return RedirectToAction("CreditCheckConfirmation", new { contractId });
            }

            if (checkResult?.Item1 == null && (checkResult?.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                return RedirectToAction("Error", "Info", new { alers = checkResult.Item2 });
            }

            switch (checkResult?.Item1.CreditCheckState)
            {                
                case CreditCheckState.Approved:
                case CreditCheckState.MoreInfoRequired:
                    TempData["MaxCreditAmount"] = checkResult?.Item1.CreditAmount;
                    return RedirectToAction("EquipmentInformation", new { contractId });
                    break;
                case CreditCheckState.Declined:
                    return View("CreditRejected", contractId);
                    break;                                
                case CreditCheckState.NotInitiated:
                case CreditCheckState.Initiated:
                default:
                    return View("CreditRejected", contractId);
            }

            ////Thread.Sleep(3000);
            //var contractResult = await _contractServiceAgent.GetContract(contractId);
            //if (contractResult.Item1 == null)
            //{
            //    return View("~/Views/Shared/Error.cshtml");
            //}
            //if (true)
            //{
            //    TempData["MaxCreditAmount"] = 15000;
            //    return RedirectToAction("EquipmentInformation", new {contractId});
            //}
            //else
            //{
            //    return View("CreditRejected", contractId);
            //}
        }

        public async Task<ActionResult> EquipmentInformation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetEquipmentInfoAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModel equipmentInfo)
        {
            ViewBag.IsAllInfoCompleted = false;
            if (!ModelState.IsValid)
            {
                ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
                return View(equipmentInfo);
            }
            var updateResult = await _contractManager.UpdateContractAsync(equipmentInfo);
            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                return RedirectToAction("Error", "Info", new { alers = updateResult });
            }
            return RedirectToAction("ContactAndPaymentInfo", new {contractId = equipmentInfo.ContractId});
        }

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
                return RedirectToAction("Error", "Info", new { alers = updateResult });
            }
            return RedirectToAction("SummaryAndConfirmation", new {contractId = contactAndPaymentInfo.ContractId});
        }

        public async Task<ActionResult> SummaryAndConfirmation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }
        
        public async Task<ActionResult> SubmitDeal(int contractId)
        {
            await _contractServiceAgent.SubmitContract(contractId);
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
            if (!string.IsNullOrEmpty(emails.HomeOwnerEmail))
            {
                var contractRes = await _contractServiceAgent.GetContract(emails.ContractId);
                if (contractRes?.Item1 != null)
                {
                    var emls = contractRes.Item1.PrimaryCustomer?.Emails;
                    if (emls?.Any(e => e.EmailType == EmailType.Notification) ?? false)
                    {
                        emls.First(e => e.EmailType == EmailType.Notification).EmailAddress =
                            emails.HomeOwnerEmail;
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
                            EmailAddress = emails.HomeOwnerEmail
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
                EmailAddress = emails.HomeOwnerEmail, Role = SignatureRole.HomeOwner
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
                return RedirectToAction("Error", "Info", new { alers = contract?.Item2 });
            }

            var sendEmails = new SendEmailsViewModel();
            sendEmails.ContractId = contractId;
            sendEmails.HomeOwnerFullName = contract.Item1.PrimaryCustomer.FirstName + " " + contract.Item1.PrimaryCustomer.LastName;
            sendEmails.HomeOwnerId = contract.Item1.PrimaryCustomer.Id;
            sendEmails.HomeOwnerEmail = contract.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                contract.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress;
            if (contract.Item1.SecondaryCustomers?.Any() ?? false)
            {
                sendEmails.AdditionalApplicantsEmails =
                    contract.Item1.SecondaryCustomers.Select(c =>
                        new CustomerEmail()
                        {
                            CustomerId = c.Id,
                            Email = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                        c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress
                        }).ToArray();
            }
            var dealer = await _dictionaryServiceAgent.GetDealerInfo();
            if (!string.IsNullOrEmpty(dealer?.Email))
            {
                sendEmails.SalesRepEmail = dealer.Email;
            }
            sendEmails.AgreementType = contract.Item1.Equipment?.AgreementType ?? AgreementType.LoanApplication;

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
                return RedirectToAction("Error", "Info", new { alers = result?.Item2 });
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

        private JsonResult GetSuccessJson()
        {
            return Json(new {isSuccess = true});
        }

        private JsonResult GetErrorJson()
        {
            return Json(new {isError = true});
        }
    }
}