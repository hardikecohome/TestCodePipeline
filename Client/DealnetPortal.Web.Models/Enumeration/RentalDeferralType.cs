using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum RentalDeferralType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "NoDeferral")]
        NoDeferral = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "TwoMonth")]
        TwoMonth = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "ThreeMonth")]
        ThreeMonth = 2
    }
}
