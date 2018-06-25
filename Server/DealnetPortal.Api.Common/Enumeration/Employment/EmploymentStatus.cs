using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration.Employment
{
    public enum EmploymentStatus
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Employed")]
        Employed = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Unemployed")]
        Unemployed = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "SelfEmployed")]
        SelfEmployed = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Retired")]
        Retired = 3
    }
}
