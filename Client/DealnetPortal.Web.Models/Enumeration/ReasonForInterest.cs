using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
