﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public virtual ICollection<Language> EnabledLanguages { get; set; }
        public virtual ICollection<DealerService> Services { get; set; }
    }
}
