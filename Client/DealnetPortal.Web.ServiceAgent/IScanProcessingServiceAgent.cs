using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Scanning;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IScanProcessingServiceAgent
    {
        Task<Tuple<DriverLicenseData, IList<Alert>>> ScanDriverLicense(ScanningRequest scanningRequest);
        Task<Tuple<VoidChequeData, IList<Alert>>> ScanVoidCheque(ScanningRequest scanningRequest);
    }
}
