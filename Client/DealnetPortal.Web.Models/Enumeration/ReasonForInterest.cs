using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum ReasonForInterest
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "UnhappyCurrentFinancing")]
        UnhappyCurrentSolution,
        [Display(ResourceType = typeof(Resources.Resources), Name = "LookingForSecond")]
        LookingForSecondary,
        [Display(ResourceType = typeof(Resources.Resources), Name = "NoCurrentFinancing")]
        Nocurrentfinancingsolution,
    }
}
