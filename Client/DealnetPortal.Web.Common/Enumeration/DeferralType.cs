using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum DeferralType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "NoDeferral")]
        NoDeferral,
        [Display(ResourceType = typeof (Resources.Resources), Name = "ThreeMonth")]
        ThreeMonth,
        [Display(ResourceType = typeof (Resources.Resources), Name = "SixMonth")]
        SixMonth
    }
}
