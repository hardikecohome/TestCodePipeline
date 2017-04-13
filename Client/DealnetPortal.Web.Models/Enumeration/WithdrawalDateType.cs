using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum WithdrawalDateType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "OneSt")]
        First,
        [Display(ResourceType = typeof (Resources.Resources), Name = "FifteenTh")]
        Fifteenth
    }
}
