using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.Dealer
{
    public class DocumentResponseViewModel
    {
        public int DealerInfoId { get; set; }

        public string AccessKey { get; set; }

        public bool IsSuccess { get; set; } = true;

        public string AggregatedError { get; set; }
    }
}