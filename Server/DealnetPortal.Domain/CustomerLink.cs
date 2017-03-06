using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class CustomerLink
    {
        public CustomerLink()
        {
            Services = new HashSet<DealerService>();
        }
        public int Id { get; set; }
        public bool IsEnglishEnabled { get; set; }
        public bool IsFrenchEnabled { get; set; }

        public virtual ICollection<DealerService> Services { get; set; }

        public Customer Customer { get; set; }
    }
}
