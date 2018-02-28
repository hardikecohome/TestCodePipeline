using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class PaymentInfoViewModel
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "PaymentType")]
        public PaymentType PaymentType { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "PrefferedWithdrawalDateIncorrectFormat")]
        public WithdrawalDateType PrefferedWithdrawalDate { get; set; }
        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "BankNumberIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "BlankNumber")]
        public string BlankNumber { get; set; }
        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TransitNumberIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "TransitNumber")]
        public string TransitNumber { get; set; }
        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [RegularExpression(@"^[0-9- ]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AccountNumberIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AccountNumber")]
        public string AccountNumber { get; set; }
        [StringLength(12, MinimumLength = 12, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "EnbridgeGasDistributionAccountMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "EnbridgeGasDistributionAccountIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EnbridgeGasDistributionAccount")]
        public string EnbridgeGasDistributionAccount { get; set; }
        [StringLength(7, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MeterNumberIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "MeterNumber")]
        public string MeterNumber { get; set; }
    }
}