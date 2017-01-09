using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum PaymentType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "EnbridgeCapital")]
        Enbridge,
        [Display(ResourceType = typeof (Resources.Resources), Name = "PapCapital")]
        Pap
    }
}
