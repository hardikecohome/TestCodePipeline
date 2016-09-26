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
        Loan = 0,
        [Description("Rental Application (HWT)")]
        RentalHWT = 1,
        [Description("Rental Application")]
        Rental = 2
    }
}
