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

        [Display(Name = "Installed Equipment Model")]
        public string Model { get; set; }

        [Display(Name = "Installed Equipment Serial Number")]
        public string SerialNumber { get; set; }
    }
}