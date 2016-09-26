using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum AgreementType
    {
        [Description("Loan Application")]
        LoanApplication = 0,
        [Description("Rental Application (HWT)")]
        RentalApplicationHwt = 1,
        [Description("Rental Application")]
        RentalApplication = 2
    }
}
