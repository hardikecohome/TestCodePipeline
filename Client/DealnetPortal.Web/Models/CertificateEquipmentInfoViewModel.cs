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

        [Display(Name = "installed equipement model")]        
        public string Model { get; set; }

        [Display(Name = "Installed equipment serial number")]
        public string SerialNumber { get; set; }
    }
}