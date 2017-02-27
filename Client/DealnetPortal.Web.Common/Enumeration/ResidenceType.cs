using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum ResidenceType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "Own")]
        Own = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "Rental")]
        Rental = 1
    }
}
