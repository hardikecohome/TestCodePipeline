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
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Extensions;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Ajax.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.Controllers
{
    using Models.EquipmentInformation;

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

        public async Task<ActionResult> EquipmentInformation()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<ActionResult> EquipmentInformation(EquipmentInformationViewModel equipmentInfo)
        {
            if (ModelState.IsValid)
            {
                return this.View();
            }
            return this.View();
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

        private async Task<IList<Alert>> UpdateContractAsync(ContractDTO contract, BasicInfoViewModel basicInfo)
        {
            var contractData = new ContractDataDTO();
            contractData.Id = basicInfo.ContractId ?? 0;
            contractData.PrimaryCustomer = AutoMapper.Mapper.Map<CustomerDTO>(basicInfo.HomeOwner);            
            if (basicInfo.AdditionalApplicants != null)
            {
                contractData.SecondaryCustomers = new List<CustomerDTO>();
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