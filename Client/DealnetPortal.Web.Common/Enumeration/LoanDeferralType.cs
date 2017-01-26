using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum LoanDeferralType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "NoDeferral")]
        NoDeferral = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "ThreeMonth")]
        ThreeMonth = 2,
        [Display(ResourceType = typeof (Resources.Resources), Name = "SixMonth")]
        SixMonth = 3,
        [Display(ResourceType = typeof (Resources.Resources), Name = "NineMonth")]
        NineMonth = 4,
        [Display(ResourceType = typeof (Resources.Resources), Name = "TwelveMonth")]
        TwelveMonth =5
    }
}
