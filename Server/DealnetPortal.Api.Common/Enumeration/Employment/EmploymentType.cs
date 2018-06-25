using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration.Employment
{
    public enum EmploymentType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "FullTime")]
        FullTime = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "PartTime")]
        PartTime = 1
    }
}
