using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models
{
    public class CertificateEquipmentInfoViewModel
    {
        public int Id { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "InstalledEquipmentModel")]
        public string Model { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "InstalledEquipmentSerialNumber")]
        public string SerialNumber { get; set; }
    }
}