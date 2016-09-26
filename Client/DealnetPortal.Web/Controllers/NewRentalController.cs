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
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;

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
            if (contractResult.Item1.SecondaryCustomers != null && contractResult.Item1.SecondaryCustomers.Any())
            {
                TempData["MaxCreditAmount"] = 100500;
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
            return View(await GetContactAndPaymentInfoAsync(contractId));
        }

        [HttpPost]
        public async Task<ActionResult> ContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
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
            equipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            MapEquipmentInfo(equipmentInfo, contractResult.Item1);
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
            MapEquipmentInfo(summaryAndConfirmation.EquipmentInfo, contractResult.Item1);
            summaryAndConfirmation.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summaryAndConfirmation.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summaryAndConfirmation.ContactAndPaymentInfo, contractResult.Item1);
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

        private void MapEquipmentInfo(EquipmentInformationViewModel equipmentInfo, ContractDTO contract)
        {
            if (equipmentInfo != null)
            {
                equipmentInfo.NewEquipment = AutoMapper.Mapper.Map<List<NewEquipmentInformation>>(contract.Equipment?.NewEquipment);
                equipmentInfo.ExistingEquipment = AutoMapper.Mapper.Map<List<ExistingEquipmentInformation>>(contract.Equipment?.ExistingEquipment);
            }
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
            if (equipmnetInfo.NewEquipment != null)
            {
                foreach (var newEquipment in equipmnetInfo.NewEquipment)
                {
                    contractData.Equipment.NewEquipment.Add(AutoMapper.Mapper.Map<NewEquipmentDTO>(newEquipment));
                }
            }
            if (equipmnetInfo.ExistingEquipment != null)
            {
                foreach (var existingEquipment in equipmnetInfo.ExistingEquipment)
                {
                    contractData.Equipment.ExistingEquipment.Add(
                        AutoMapper.Mapper.Map<ExistingEquipmentDTO>(existingEquipment));
                }
            }
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