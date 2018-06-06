using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum WithdrawalDateType
    {
        [Display(Name = "1st")]
        First,
        [Display(Name = "15th")]
        Fifteenth
    }
}
