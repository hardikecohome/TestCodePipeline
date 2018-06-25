using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Common.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{
    public class ContractViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        public AdditionalInfoViewModel AdditionalInfo { get; set; }
        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }
        public LoanCalculator.Output LoanCalculatorOutput { get; set; }
        public UploadDocumentsViewModel UploadDocumentsInfo { get; set; }
        public PaymentSummary PaymentSummary { get; set; }
        public bool IsClarityDealer { get; set; }
        public bool IsOldClarityDeal { get; set; }
    }
}
