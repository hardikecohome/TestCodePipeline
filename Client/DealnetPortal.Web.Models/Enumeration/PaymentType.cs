using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum PaymentType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "EnbridgeCapital")]
        Enbridge,
        [Display(ResourceType = typeof (Resources.Resources), Name = "PapCapital")]
        Pap
    }
}
