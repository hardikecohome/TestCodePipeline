using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum SignatureStatus
    {
        NotInitiated = 0,
        Draft = 1,
        Sent = 2,
        Signed = 3,
        Completed = 4,
        Declined = 5,
        Deleted = 6
    }
}
