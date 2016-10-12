using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum PaymentType
    {
        [Display(Name = "ENBRIDGE")]
        [Description("ENBRIDGE")]
        Enbridge,
        [Display(Name = "PAP")]
        [Description("PAP")]
        Pap
    }
}
