using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;

using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    [Authorize(Roles = "Dealer")]
    public class UpdateDataController : UpdateController
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
        public async Task<JsonResult> UpdateApplicants(ApplicantsViewModel basicInfo)
        {
            if (!ModelState.IsValid || basicInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await _contractManager.UpdateApplicants(basicInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateEquipmentInfo(EquipmentInformationViewModel equipmentInfo)
        {
            if (equipmentInfo.ContractId == null)
            {
                return GetErrorJson();
            }
            var updateResult = await _contractManager.UpdateContractAsync(equipmentInfo);
            return updateResult.Any(r => r.Type == AlertType.Error) ? GetErrorJson() : GetSuccessJson();
        }
    }
}