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

        public AgreementType? AgreementType { get; set; }

        public string State { get; set; }

        public string DealerId { get; set; }
        [ForeignKey("DealerId")]
        public ApplicationUser Dealer { get; set; }

        public byte[] AgreementForm { get; set; }

        public string ExternalTemplateId { get; set; }

        public string EquipmentType { get; set; }

        //public virtual List<EquipmentType> EquipmentTypes { get; set; }
    }
}
