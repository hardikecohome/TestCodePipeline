using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum SignatureStatus
    {
        [Display(ResourceType =typeof(Resources.Resources), Name ="NotInitiated")]
        NotInitiated = 0,
        [Display(ResourceType =typeof(Resources.Resources), Name ="Created")]
        Created = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Sent")]
        Sent = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Delivered")]
        Delivered =3,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Signed")]
        Signed = 4,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Completed")]
        Completed = 5,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Declined")]
        Declined = 6,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Deleted")]
        Deleted = 7
    }
}
