using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            EnabledLanguages = new HashSet<Language>();
        }
        public int Id { get; set; }

        public virtual ICollection<Language> EnabledLanguages { get; set; }
        public virtual ICollection<DealerService> Services { get; set; }
        public ApplicationUser User { get; set; }
    }
}
