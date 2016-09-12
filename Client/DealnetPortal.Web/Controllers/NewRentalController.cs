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
                await UpdateContractAsync(contractResult.Item1, basicInfo);
            }
            return RedirectToAction("CreditCheckConfirmation", new { contractId = contractResult?.Item1?.Id ?? 0 });
        }

        public async Task<ActionResult> CreditCheckConfirmation(int contractId)
        {
            return View(await GetBasicInfoAsync(contractId));
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
            basicInfo.HomeOwner = contractResult.Item1.Customers?.FirstOrDefault()?.ToApplicantPersonalInfo();
            if (contractResult.Item1.Customers?.Count() > 1)
            {
                basicInfo.AdditionalApplicants = contractResult.Item1.Customers?.Skip(1).Select(c => c.ToApplicantPersonalInfo()).ToList();
            }
            basicInfo.AddressInformation = contractResult.Item1.Addresses?.FirstOrDefault(a => a.AddressType == AddressType.MainAddress)?.ToContractAddressDto();
            var mailingAddress = contractResult.Item1.Addresses?.FirstOrDefault(a => a.AddressType == AddressType.MailAddress);
            if (mailingAddress != null)
            {
                basicInfo.MailingAddressInformation = mailingAddress.ToContractAddressDto();
            }
            return basicInfo;
        }

        private async Task UpdateContractAsync(ContractDTO contract, BasicInfoViewModel basicInfo)
        {
            contract.Customers = new List<CustomerDTO>();
            contract.Customers.Add(basicInfo.HomeOwner.ToCustomerDto());
            if (basicInfo.AdditionalApplicants != null)
            {
                contract.Customers.AddRange(basicInfo.AdditionalApplicants.Select(app => app.ToCustomerDto()));
            }
            contract.Addresses.Add(basicInfo.AddressInformation.ToContractAddressDto(AddressType.MainAddress));
            if (basicInfo.MailingAddressInformation != null)
            {
                contract.Addresses.Add(basicInfo.MailingAddressInformation.ToContractAddressDto(AddressType.MailAddress));
            }
            await _contractServiceAgent.UpdateContractClientData(contract);
        }

        private JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}