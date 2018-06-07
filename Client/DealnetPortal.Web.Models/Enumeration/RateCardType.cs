using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum RateCardType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "FixedRate")]
        FixedRate = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "NoInterest")]
        NoInterest = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Defferal")]
        Defferal = 2
    }
}
