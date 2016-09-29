using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Models.Storage
{
    public class AgreementTemplateDTO
    {
        public int Id { get; set; }

        public string TemplateName { get; set; }

        public AgreementType AgreementType { get; set; }

        public string State { get; set; }

        public byte[] AgreementFormRaw { get; set; }

        //public virtual List<string> EquipmentTypes { get; set; }
    }
}
