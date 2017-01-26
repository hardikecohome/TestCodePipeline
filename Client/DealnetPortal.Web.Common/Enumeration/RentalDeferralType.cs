using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum RentalDeferralType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "NoDeferral")]
        NoDeferral = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "TwoMonth")]
        TwoMonth = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "ThreeMonth")]
        ThreeMonth = 2
    }
}
