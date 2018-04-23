using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum AgreementType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanApplication")]
        LoanApplication = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "LeaseApplicationHwt")]
        RentalApplicationHwt = 1,
        [Display(ResourceType = typeof (Resources.Resources), Name = "LeaseApplication")]
        RentalApplication = 2
    }
}
