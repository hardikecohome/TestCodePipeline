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

        public int? CustomerCommentId { get; set; }
        [ForeignKey("CustomerCommentId")]
        public Comment CustomerComment { get; set; }

        public int? SelectedServiceId { get; set; }
        [ForeignKey("SelectedServiceId")]
        public DealerService SelectedService { get; set; }
    }
}
