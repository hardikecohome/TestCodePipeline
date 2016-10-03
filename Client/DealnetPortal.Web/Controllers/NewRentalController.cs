using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
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
    public class NewRentalController : Controller
    {
        private readonly IScanProcessingServiceAgent _scanProcessingServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;

        public NewRentalController(IScanProcessingServiceAgent scanProcessingServiceAgent, IContractServiceAgent contractServiceAgent)
        {
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
            _contractServiceAgent = contractServiceAgent;
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
                return View(await GetBasicInfoAsync(contractId.Value));
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
                var updateResult = await UpdateContractAsync(basicInfo);
                if (updateResult.Any(r => r.Type == AlertType.Error))
                {
                    return View("~/Views/Shared/Error.cshtml");
                }
            }
            return RedirectToAction("CreditCheckConfirmation", new { contractId = contractResult?.Item1?.Id ?? 0 });
        }

        public async Task<ActionResult> CreditCheckConfirmation(int contractId)
        {
            return View(await GetBasicInfoAsync(contractId));
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
            return View(await GetEquipmentInfoAsync(contractId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModel equipmentInfo)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var updateResult = await UpdateContractAsync(equipmentInfo);
            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            return RedirectToAction("ContactAndPaymentInfo", new {contractId = equipmentInfo.ContractId});
        }

        public async Task<ActionResult> ContactAndPaymentInfo(int contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            return View(await GetContactAndPaymentInfoAsync(contractId));
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
            var updateResult = await UpdateContractAsync(contactAndPaymentInfo);
            if (updateResult.Any(r => r.Type == AlertType.Error))
            {
                return View("~/Views/Shared/Error.cshtml");
            }
            return RedirectToAction("SummaryAndConfirmation", new { contractId = contactAndPaymentInfo.ContractId });
        }

        public async Task<ActionResult> SummaryAndConfirmation(int contractId)
        {
            ViewBag.EquipmentTypes = (await _contractServiceAgent.GetEquipmentTypes()).Item1;
            return View(await GetSummaryAndConfirmationAsync(contractId));
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
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateBasicInfo(BasicInfoViewModel basicInfo)
        {
            if (!ModelState.IsValid || basicInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await UpdateContractAsync(basicInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            if (!ModelState.IsValid || contactAndPaymentInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await UpdateContractAsync(contactAndPaymentInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateEquipmentInfo(EquipmentInformationViewModel equipmentInfo)
        {
            if (!ModelState.IsValid || equipmentInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await UpdateContractAsync(equipmentInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
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

        private async Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId)
        {
            var basicInfo = new BasicInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return basicInfo;
            }
            basicInfo.ContractId = contractId;
            MapBasicInfo(basicInfo, contractResult.Item1);
            return basicInfo;
        }

        private async Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId)
        {
            var contactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return contactAndPaymentInfo;
            }
            contactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(contactAndPaymentInfo, contractResult.Item1);
            return contactAndPaymentInfo;
        }

        private async Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId)
        {
            var equipmentInfo = new EquipmentInformationViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return equipmentInfo;
            }
            equipmentInfo.ContractId = contractId;
            if (contractResult.Item1.Equipment != null)
            {
                equipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            }
            return equipmentInfo;
        }

        private async Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId)
        {
            var summaryAndConfirmation = new SummaryAndConfirmationViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return summaryAndConfirmation;
            }
            summaryAndConfirmation.BasicInfo = new BasicInfoViewModel();
            summaryAndConfirmation.BasicInfo.ContractId = contractId;
            MapBasicInfo(summaryAndConfirmation.BasicInfo, contractResult.Item1);
            summaryAndConfirmation.EquipmentInfo = new EquipmentInformationViewModel();
            summaryAndConfirmation.EquipmentInfo.ContractId = contractId;
            summaryAndConfirmation.EquipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            summaryAndConfirmation.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summaryAndConfirmation.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summaryAndConfirmation.ContactAndPaymentInfo, contractResult.Item1);
            summaryAndConfirmation.SendEmails = new SendEmailsViewModel();
            var rate = (await _contractServiceAgent.GetProvinceTaxRate(summaryAndConfirmation.BasicInfo.AddressInformation.Province.ToProvinceAbbreviation())).Item1;
            if (rate != null) { summaryAndConfirmation.ProvinceTaxRate = rate.Rate; }
            summaryAndConfirmation.SendEmails.ContractId = contractId;
            summaryAndConfirmation.SendEmails.HomeOwnerFullName = summaryAndConfirmation.BasicInfo.HomeOwner.FirstName + " " + summaryAndConfirmation.BasicInfo.HomeOwner.LastName;
            return summaryAndConfirmation;
        }

        private void MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract)
        {
            basicInfo.HomeOwner = AutoMapper.Mapper.Map<ApplicantPersonalInfo>(contract.PrimaryCustomer);
            basicInfo.AdditionalApplicants = AutoMapper.Mapper.Map<List<ApplicantPersonalInfo>>(contract.SecondaryCustomers);

            basicInfo.AddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MainAddress));
            basicInfo.MailingAddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MailAddress));
        }

        private void MapContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo, ContractDTO contract)
        {
            contactAndPaymentInfo.PaymentInfo = AutoMapper.Mapper.Map<PaymentInfoViewModel>(
                    contract.PaymentInfo);
            contactAndPaymentInfo.ContactInfo = AutoMapper.Mapper.Map<ContactInfoViewModel>(
                    contract.ContactInfo);
            if (contract.ContactInfo?.Phones != null)
            {
                foreach (var phone in contract.ContactInfo.Phones)
                {
                    switch (phone.PhoneType)
                    {
                        case PhoneType.Home:
                            contactAndPaymentInfo.ContactInfo.HomePhone = phone.PhoneNum;
                            break;
                        case PhoneType.Cell:
                            contactAndPaymentInfo.ContactInfo.CellPhone = phone.PhoneNum;
                            break;
                        case PhoneType.Business:
                            contactAndPaymentInfo.ContactInfo.BusinessPhone = phone.PhoneNum;
                            break;
                    }
                }
            }
        }

        private async Task<IList<Alert>> UpdateContractAsync(BasicInfoViewModel basicInfo)
        {
            var contractData = new ContractDataDTO();
            contractData.Id = basicInfo.ContractId ?? 0;
            contractData.PrimaryCustomer = AutoMapper.Mapper.Map<CustomerDTO>(basicInfo.HomeOwner);
            contractData.SecondaryCustomers = new List<CustomerDTO>();
            if (basicInfo.AdditionalApplicants != null)
            {
                basicInfo.AdditionalApplicants.ForEach(a =>
                {
                    contractData.SecondaryCustomers.Add(AutoMapper.Mapper.Map<CustomerDTO>(a));
                });
            }
            contractData.Locations = new List<LocationDTO>();
            var address = AutoMapper.Mapper.Map<LocationDTO>(basicInfo.AddressInformation);
            address.AddressType = AddressType.MainAddress;
            contractData.Locations.Add(address);
            if (basicInfo.MailingAddressInformation != null)
            {
                address = AutoMapper.Mapper.Map<LocationDTO>(basicInfo.MailingAddressInformation);
                address.AddressType = AddressType.MailAddress;
                contractData.Locations.Add(address);
            }
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        private async Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                Equipment = AutoMapper.Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        private async Task<IList<Alert>> UpdateContractAsync(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            var contractData = new ContractDataDTO();
            contractData.Id = contactAndPaymentInfo.ContractId ?? 0;
            if (contactAndPaymentInfo.ContactInfo != null)
            {
                var contactInfo = AutoMapper.Mapper.Map<ContactInfoDTO>(contactAndPaymentInfo.ContactInfo);
                if (contactAndPaymentInfo.ContactInfo.HomePhone != null ||
                    contactAndPaymentInfo.ContactInfo.CellPhone != null ||
                    contactAndPaymentInfo.ContactInfo.BusinessPhone != null)
                {
                    contactInfo.Phones = new List<PhoneDTO>();
                }
                if (contactAndPaymentInfo.ContactInfo.HomePhone != null)
                {
                    contactInfo.Phones.Add(new PhoneDTO
                    {
                        PhoneNum = contactAndPaymentInfo.ContactInfo.HomePhone,
                        PhoneType = PhoneType.Home
                    });
                }
                if (contactAndPaymentInfo.ContactInfo.CellPhone != null)
                {
                    contactInfo.Phones.Add(new PhoneDTO
                    {
                        PhoneNum = contactAndPaymentInfo.ContactInfo.CellPhone,
                        PhoneType = PhoneType.Cell
                    });
                }
                if (contactAndPaymentInfo.ContactInfo.BusinessPhone != null)
                {
                    contactInfo.Phones.Add(new PhoneDTO
                    {
                        PhoneNum = contactAndPaymentInfo.ContactInfo.BusinessPhone,
                        PhoneType = PhoneType.Business
                    });
                }
                contractData.ContactInfo = contactInfo;
            }
            else
            {
                contractData.ContactInfo = new ContactInfoDTO();
            }
            if (contactAndPaymentInfo.PaymentInfo != null)
            {
                var paymentInfo = AutoMapper.Mapper.Map<PaymentInfoDTO>(contactAndPaymentInfo.PaymentInfo);
                contractData.PaymentInfo = paymentInfo;
            }
            return await _contractServiceAgent.UpdateContractData(contractData);
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