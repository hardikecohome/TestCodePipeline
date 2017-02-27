using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum WithdrawalDateType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "OneSt")]
        First,
        [Display(ResourceType = typeof (Resources.Resources), Name = "FifteenTh")]
        Fifteenth
    }
}
