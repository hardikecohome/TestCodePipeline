using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IDocumentProcessingServiceAgent
    {
        Task<Tuple<DriverLicenseData, IList<Alert>>> GetDriverLicense(ScanningRequest scanningRequest);
    }
}
