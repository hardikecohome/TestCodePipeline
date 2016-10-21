using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class UpdateDataController : Controller
    {
        private readonly IContractManager _contractManager;

        public UpdateDataController(IContractManager contractManager)
        {
            _contractManager = contractManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateBasicInfo(BasicInfoViewModel basicInfo)
        {
            if (!ModelState.IsValid || basicInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await _contractManager.UpdateContractAsync(basicInfo);
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
            var updateResult = await _contractManager.UpdateContractAsync(contactAndPaymentInfo);
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
            var updateResult = await _contractManager.UpdateContractAsync(equipmentInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }

        protected JsonResult GetSuccessJson()
        {
            return Json(new { isSuccess = true });
        }

        protected JsonResult GetErrorJson()
        {
            return Json(new { isError = true });
        }
    }
}