using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Attributes;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum DeferralType
    {
        [Display(Name = "No Deferral")]
        [Description("No Deferral")]
        [PersistentDescription("No Deferral")]
        NoDeferral,
        [Display(Name = "3 Month")]
        [Description("3 Month")]
        [PersistentDescription("3 Month")]
        ThreeMonth,
        [Display(Name = "6 Month")]
        [Description("6 Month")]
        [PersistentDescription("6 Month")]
        SixMonth       
    }
}
