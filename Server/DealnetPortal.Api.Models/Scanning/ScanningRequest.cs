using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Scanning
{
    public class ScanningRequest
    {
        public string OperationId { get; set; }
        public byte[] ImageForReadRaw { get; set; }
    }
}
