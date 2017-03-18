using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models
{
    public class CertificateInformationViewModel
    {
        public int ContractId { get; set; }

        public List<CertificateEquipmentInfoViewModel> Equipments { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "InstallerFirstName")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessage = "First Name is in incorrect format")]
        public string InstallerFirstName { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "InstallerLastName")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "LastNameIncorrectFormat")]
        public string InstallerLastName { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "InstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? InstallationDate { get; set; }
    }
}