using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
using Microsoft.Ajax.Utilities;
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
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModel equipmentInfo)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();
            if (!ModelState.IsValid)
            {
                return View();
            }
            var contractResult = equipmentInfo.ContractId == null ?
                await _contractServiceAgent.CreateContract() :
                await _contractServiceAgent.GetContract(equipmentInfo.ContractId.Value);
            if (contractResult?.Item1 != null)
            {
                var updateResult = await UpdateContractAsync(contractResult.Item1, equipmentInfo);
                if (updateResult.Any(r => r.Type == AlertType.Error))
                {
                    return View();
                }
            }
            return RedirectToAction("CreditCheckConfirmation", new { contractId = contractResult?.Item1?.Id ?? 0 });
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
                var updateResult = await UpdateContractAsync(contractResult.Item1, basicInfo);
                if (updateResult.Any(r => r.Type == AlertType.Error))
                {
                    return View();
                }
            }
            return RedirectToAction("CreditCheckConfirmation", new { contractId = contractResult?.Item1?.Id ?? 0 });
        }

        public async Task<ActionResult> CreditCheckConfirmation(int contractId)
        {
            return View(await GetBasicInfoAsync(contractId));
        }

        public async Task<ActionResult> EquipmentInformation(int? contractId)
        {
            ViewBag.IsMobileRequest = HttpContext.Request.IsMobileBrowser();

            //if (contractId == null)
            //{
            //    var contract = await _contractServiceAgent.CreateContract();
            //    if (contract?.Item1 != null)
            //    {
            //        contractId = contract.Item1.Id;
            //    }
            //}
            if (contractId != null)
            {
                return View(await this.GetEquipmentInfoAsync(contractId.Value));
            }
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> UpdateBasicInfo(BasicInfoViewModel basicInfo)
        {
            if (!ModelState.IsValid || basicInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var contractResult = await _contractServiceAgent.GetContract(basicInfo.ContractId.Value);
            if (contractResult?.Item1 == null) return GetErrorJson();
            var updateResult = await UpdateContractAsync(contractResult.Item1, basicInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
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
                return View();
            }
            return View(new ContactAndPaymentInfoViewModel()); //TODO: Navigate to next step
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
            basicInfo.HomeOwner = AutoMapper.Mapper.Map<ApplicantPersonalInfo>(contractResult.Item1.PrimaryCustomer);
            basicInfo.AdditionalApplicants = AutoMapper.Mapper.Map<List<ApplicantPersonalInfo>>(contractResult.Item1.SecondaryCustomers);

            basicInfo.AddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contractResult.Item1.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MainAddress));
            basicInfo.MailingAddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contractResult.Item1.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MailAddress));
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
            contactAndPaymentInfo.PaymentInfo = AutoMapper.Mapper.Map<PaymentInfoViewModel>(
                    contractResult.Item1.PaymentInfo);
            contactAndPaymentInfo.ContactInfo = AutoMapper.Mapper.Map<ContactInfoViewModel>(
                    contractResult.Item1.ContactInfo);
            if (contractResult.Item1.ContactInfo?.Phones != null)
            {
                foreach (var phone in contractResult.Item1.ContactInfo.Phones)
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
            return contactAndPaymentInfo;
        }

        private async Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId)
        {
            var equipmentInfo = new EquipmentInformationViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1?.Equipment == null)
            {
                equipmentInfo.ContractId = contractId;
                return equipmentInfo;
            }
            equipmentInfo.ContractId = contractId;
            equipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            equipmentInfo.NewEquipment = AutoMapper.Mapper.Map<List<NewEquipmentInformation>>(contractResult.Item1.Equipment.NewEquipment);
            equipmentInfo.ExistingEquipment = AutoMapper.Mapper.Map<List<ExistingEquipmentInformation>>(contractResult.Item1.Equipment.ExistingEquipment);
            return equipmentInfo;
        }

        private async Task<IList<Alert>> UpdateContractAsync(ContractDTO contract, BasicInfoViewModel basicInfo)
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

        private async Task<IList<Alert>> UpdateContractAsync(ContractDTO contract, EquipmentInformationViewModel equipmnetInfo)
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
            return await this._contractServiceAgent.UpdateContractData(contractData);
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