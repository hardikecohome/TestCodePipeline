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
                    return View("~/Views/Shared/Error.cshtml");
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
            const int numOfAttempts = 3;
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

            if (checkResult?.Item1 == null && (checkResult?.Item2?.Any(a => a.Type == AlertType.Error) ?? false))
            {
                return View("~/Views/Shared/Error.cshtml");
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
                return View();
            }
            var updateResult = await _contractManager.UpdateContractAsync(equipmentInfo);
            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                return View("~/Views/Shared/Error.cshtml");
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
                return View("~/Views/Shared/Error.cshtml");
            }
            return RedirectToAction("SummaryAndConfirmation", new {contractId = contactAndPaymentInfo.ContractId});
        }

        public async Task<ActionResult> SummaryAndConfirmation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void SendContractEmails([Bind(Prefix = "SendEmails")] SendEmailsViewModel emails)
        {
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

            _contractServiceAgent.InitiateDigitalSignature(signatureUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RentalAgreementSubmitSuccess([Bind(Prefix = "SendEmails")] SendEmailsViewModel emails)
        {
            await _contractServiceAgent.SubmitContract(emails.ContractId);
            ViewBag.HomeOwnerEmail = emails.HomeOwnerEmail;
            return View(emails);
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