using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class CustomerContractInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Comment { get; set; }

        public int? DealerServiceId { get; set; }
        [ForeignKey("DealerServiceId")]
        public DealerService DealerService { get; set; }
    }
}
