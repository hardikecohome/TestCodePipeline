using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    [Flags]
    public enum AgreementType
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanApplication")]
        LoanApplication = 0,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplicationHwt")]
        RentalApplicationHwt = 1,
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalApplication")]
        RentalApplication = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "RentalApplication")]
        Rental = RentalApplicationHwt | RentalApplication,
    }
}
