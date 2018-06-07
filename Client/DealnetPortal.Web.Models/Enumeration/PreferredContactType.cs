using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum PreferredContactType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        Phone = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        Email = 1
    }
}
