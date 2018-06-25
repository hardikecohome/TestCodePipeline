using System.ComponentModel.DataAnnotations;

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