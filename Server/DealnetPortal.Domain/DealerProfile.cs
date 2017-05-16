using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class DealerProfile
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        //[ForeignKey("DealerId")]
        //public virtual ApplicationUser Dealer { get; set; }

        public virtual ICollection<DealerEquipment> Equipments { get; set; }

        public virtual ICollection<DealerArea> Areas { get; set; }

       
    }
}
