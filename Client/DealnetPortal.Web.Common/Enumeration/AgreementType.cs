using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Enumeration
{
    public enum AgreementType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanApplication")]
        LoanApplication = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplicationHwt")]
        RentalApplicationHwt = 1,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplication")]
        RentalApplication = 2
    }
}
