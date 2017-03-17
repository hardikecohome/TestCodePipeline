using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class DealerDTO : CustomerDTO
    {
        public string ParentDealerUserName { get; set; }
    }
}
