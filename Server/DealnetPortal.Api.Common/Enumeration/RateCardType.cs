using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum RateCardType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "StandardRate")]
        FixedRate = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "EqualPayments")]
        NoInterest = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Deferral")]
        Deferral = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Custom")]
        Custom = 3 
    }
}
