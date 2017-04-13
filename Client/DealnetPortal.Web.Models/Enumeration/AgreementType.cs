using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum AgreementType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanApplication")]
        LoanApplication = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplicationHwt")]
        RentalApplicationHwt = 1,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplication")]
        RentalApplication = 2
    }
}
