using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum LoanDeferralType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "NoDeferral")]
        NoDeferral = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "ThreeMonth")]
        ThreeMonth = 2,
        [Display(ResourceType = typeof (Resources.Resources), Name = "SixMonth")]
        SixMonth = 3,
        [Display(ResourceType = typeof (Resources.Resources), Name = "NineMonth")]
        NineMonth = 4,
        [Display(ResourceType = typeof (Resources.Resources), Name = "TwelveMonth")]
        TwelveMonth =5
    }
}
