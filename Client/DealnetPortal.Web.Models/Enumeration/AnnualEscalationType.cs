using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum AnnualEscalationType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Escalation_35")]
        Escalation35 = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Escalation_0")]
        Escalation0 = 0
    }
}
