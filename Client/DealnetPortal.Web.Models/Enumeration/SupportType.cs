using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum SupportType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "DealerProfileUpdate")]
        dealerProfileUpdate = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "CreditDecision")]
        creditDecision = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "PendingDeals")]
        pendingDeals = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "FundedDeals")]
        fundedDeals = 3,
        [Display(ResourceType = typeof(Resources.Resources), Name = "ProgramInquiries")]
        programInquiries = 4,
        [Display(ResourceType = typeof(Resources.Resources), Name = "PortalInquiries")]
        portalInquiries = 5,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Other")]
        Other = 6
    }
}
