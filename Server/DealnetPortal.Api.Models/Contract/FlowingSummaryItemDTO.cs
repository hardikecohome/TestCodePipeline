using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class FlowingSummaryItemDTO
    {
        public string ItemLabel { get; set; }

        public IList<int> ItemData { get; set; }
    }
}
