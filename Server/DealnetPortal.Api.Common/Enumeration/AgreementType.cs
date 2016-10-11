using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum AgreementType
    {
        [Display(Name = "Loan Application")]
        [Description("Loan Application")]
        LoanApplication = 0,
        [Display(Name = "Rental Application (HWT)")]
        [Description("Rental Application (HWT)")]
        RentalApplicationHwt = 1,
        [Display(Name = "Rental Application")]
        [Description("Rental Application")]
        RentalApplication = 2
    }
}
