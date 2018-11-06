using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum EmploymentType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name ="FullTime")]
        FullTime,
        [Display(ResourceType =typeof(Resources.Resources),Name ="PartTime")]
        PartTime
    }
}
