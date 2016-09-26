using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Api.Integration.Services
{
    /// <summary>
    /// Service for e-signature of contracts
    /// </summary>
    public interface ISignatureService
    {
        /// <summary>
        /// Initiate contract signature process
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="ownerUserId"></param>
        /// <returns></returns>
        IList<Alert> ProcessContract(int contractId, string ownerUserId);
    }
}
