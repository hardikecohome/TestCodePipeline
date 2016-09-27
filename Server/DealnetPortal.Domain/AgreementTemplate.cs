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
    public class AgreementTemplate
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public AgreementType AgreementType { get; set; }

        [MaxLength(10)]
        public string State { get; set; }

        public byte[] AgreementForm { get; set; }
        
        //public int? EquipmentTypeId { get; set; }
        //[ForeignKey("EquipmentTypeId")]
        public virtual List<EquipmentType> EquipmentType { get; set; }
    }
}
