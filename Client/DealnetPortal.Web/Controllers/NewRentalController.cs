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

        public NewRentalController(IScanProcessingServiceAgent scanProcessingServiceAgent, IContractServiceAgent contractServiceAgent, IContractManager contractManager) : base(contractManager)
        {
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
            _contractServiceAgent = contractServiceAgent;
            _contractManager = contractManager;
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
            }
            if (contractId != null)
            {
                return View(await _contractManager.GetBasicInfoAsync(contractId.Value));
            }
            return View();
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
        public ActionResult CreditCheckConfirmation(BasicInfoViewModel basicInfo)
        {
            return RedirectToAction("CreditCheck", new { contractId = basicInfo.ContractId });
        }

        public ActionResult CreditCheck(int contractId)
        {
            return View(contractId);
        }

        public async Task<ActionResult> CheckCreditStatus(int contractId)
        {
            //TODO: Initiate real credit status check
            Thread.Sleep(3000);
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            if (true)
            {
                TempData["MaxCreditAmount"] = 15000;
                return RedirectToAction("EquipmentInformation", new {contractId});
            }
            else
            {
                return View("CreditRejected", contractId);
            }
        }

        public async Task<ActionResult> EquipmentInformation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _contractServiceAgent.GetEquipmentTypes()).Item1;
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
            return RedirectToAction("SummaryAndConfirmation", new { contractId = contactAndPaymentInfo.ContractId });
        }

        public async Task<ActionResult> SummaryAndConfirmation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _contractServiceAgent.GetEquipmentTypes()).Item1;
            return View(await _contractManager.GetSummaryAndConfirmationAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public void SendContractEmails([Bind(Prefix = "SendEmails")]SendEmailsViewModel emails)
        {
            SignatureUsersDTO signatureUsers = new SignatureUsersDTO();
            signatureUsers.ContractId = emails.ContractId;
            signatureUsers.Users = new List<SignatureUser>();
            signatureUsers.Users.Add(new SignatureUser()
            {
                EmailAddress = emails.HomeOwnerEmail,
                Role = SignatureRole.HomeOwner
            });

            emails.AdditionalApplicantsEmails?.ForEach(us =>
            {
                signatureUsers.Users.Add(new SignatureUser()
                {
                    EmailAddress = us.Email,
                    Role = SignatureRole.AdditionalApplicant
                });
            });

            _contractServiceAgent.InitiateDigitalSignature(signatureUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RentalAgreementSubmitSuccess([Bind(Prefix = "SendEmails")]SendEmailsViewModel emails)
        {
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
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };
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
            ScanningRequest scanningRequest = new ScanningRequest() { ImageForReadRaw = data };
            var result = await _scanProcessingServiceAgent.ScanVoidCheque(scanningRequest);
            return result.Item2.Any(x => x.Type == AlertType.Error) ? GetErrorJson() : Json(result.Item1);
        }

        private JsonResult GetSuccessJson()
        {
            return Json(new { isSuccess = true });
        }

        private JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}