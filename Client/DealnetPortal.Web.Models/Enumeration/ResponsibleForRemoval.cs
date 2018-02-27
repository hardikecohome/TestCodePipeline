using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum ResponsibleForRemoval
    {
        [Display(ResourceType =typeof(Resources.Resources),Name ="Customer")]
        Customer = 0,
        [Display(ResourceType = typeof(Resources.Resources),Name ="ExistingSupplier")]
        ExistingSupplier = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name ="N_A")]
        NA = 2,
        [Display(ResourceType = typeof(Resources.Resources),Name ="Other")]
        Other =3
    }
}
