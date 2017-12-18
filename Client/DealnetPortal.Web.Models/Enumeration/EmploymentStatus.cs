using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum EmploymentStatus
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Employed")]
        Employed,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Unemployed")]
        Unemployed,
        [Display(ResourceType = typeof(Resources.Resources), Name = "SelfEmployed")]
        SelfEmployed,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Retired")]
        Retired
    }
}
