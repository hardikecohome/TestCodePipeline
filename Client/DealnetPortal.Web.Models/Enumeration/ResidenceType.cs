using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum ResidenceType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "Own")]
        Own = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "Rental")]
        Rental = 1
    }
}
