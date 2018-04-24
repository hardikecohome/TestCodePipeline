using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum AnnualEscalationType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Escalation_0")]
        Escalation0 = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Escalation_35")]
        Escalation35 = 1
    }
}
