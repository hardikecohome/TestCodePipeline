using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum SupportType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "CreditDecision")]
        creditFunding = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "CreditDocs")]
        customerService = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "FundingDocs")]
        dealerSupport = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Other")]
        Other = 3
    }
}
