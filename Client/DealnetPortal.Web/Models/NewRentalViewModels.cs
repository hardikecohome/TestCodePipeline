using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class ApplicantPersonalInfo
    {
        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime BirthDate { get; set; }
    }

    public class AddressInformation
    {
        [Required]
        [Display(Name = "Installation Address")]
        public string InstallationAddress { get; set; }
        public string UnitNumber { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Province { get; set; }
        [Required]
        [Display(Name = "Postal Code")]
        public string PostalCode { get; set; }
    }

    //TODO: AntiForgeryToken
    public class BasicInfoViewModel
    {
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public ApplicantPersonalInfo[] AdditionalApplicants { get; set; }
        public AddressInformation AddressInformation { get; set; }
        public bool MailingAddressDiffers { get; set; }
        public AddressInformation MailingAddressInformation { get; set; }
    }
}
