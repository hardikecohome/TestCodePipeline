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
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘`-]+$", ErrorMessage = "First Name is in incorrect format")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘`-]+$", ErrorMessage = "Last Name is in incorrect format")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime BirthDate { get; set; }
        public bool AgrreesToSendPersonalInfo { get; set; }
    }

    public class AddressInformation
    {
        [Required]
        [Display(Name = "Installation Address")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘`-]+$", ErrorMessage = "Installation Address is in incorrect format")]
        public string InstallationAddress { get; set; }
        [Display(Name = "Unit Number")]
        [StringLength(10, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Unit Number is in incorrect format")]
        public string UnitNumber { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘`-]+$", ErrorMessage = "City is in incorrect format")]
        public string City { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘`-]+$", ErrorMessage = "Province is in incorrect format")]
        public string Province { get; set; }
        [Required]
        [Display(Name = "Postal Code")]
        [StringLength(10, MinimumLength = 3)]
        [RegularExpression(@"^[a-zA-Z0-9]+\s?[a-zA-Z0-9]+$", ErrorMessage = "Postal Code is in incorrect format")]
        public string PostalCode { get; set; }
    }

    //TODO: AntiForgeryToken
    public class BasicInfoViewModel
    {
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public List<ApplicantPersonalInfo> AdditionalApplicants { get; set; }
        public AddressInformation AddressInformation { get; set; }
        public AddressInformation MailingAddressInformation { get; set; }
        public int? ContractId { get; set; }
    }
}
