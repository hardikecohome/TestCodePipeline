using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{
    public class ApplicantPersonalInfo
    {
        public int? CustomerId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessage = "First Name is in incorrect format")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessage = "Last Name is in incorrect format")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Birth Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? BirthDate { get; set; }
    }

    public class AddressInformation
    {
        [Required]
        [Display(Name = "Installation Address")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.,‘'`-]+$", ErrorMessage = "Installation Address is in incorrect format")]
        public string InstallationAddress { get; set; }
        [Display(Name = "Unit #")]
        [StringLength(10, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessage = "Unit Number is in incorrect format")]
        public string UnitNumber { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessage = "City is in incorrect format")]
        public string City { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessage = "Province is in incorrect format")]
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

    public enum PaymentType
    {
        [Display(Name = "ENBRIDGE")]
        Enbridge,
        [Display(Name = "PAP")]
        Pap
    }

    public enum WithdrawalDateType
    {
        [Display(Name = "1st")]
        First,
        [Display(Name = "15th")]
        Fifteenth
    }

    public class PaymentInfoViewModel
    {
        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; }
        [Display(Name = "Preferred Withdrawal Date")]
        public WithdrawalDateType PrefferedWithdrawalDate { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessage = "Bank Number is in incorrect format")]
        [Display(Name = "Bank Number")]
        public string BlankNumber { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessage = "Transit Number is in incorrect format")]
        [Display(Name = "Transit Number")]
        public string TransitNumber { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9- ]+$", ErrorMessage = "Account Number is in incorrect format")]
        [Display(Name = "Account Number")]
        public string AccountNumber { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessage = "Enbridge Gas Distribution Account is in incorrect format")]
        [Display(Name = "Enbridge Gas Distribution Account")]
        public string EnbridgeGasDistributionAccount { get; set; }
        [StringLength(7)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Meter # is in incorrect format")]
        [Display(Name = "Meter #")]
        public string MeterNumber { get; set; }
    }

    public class ContactInfoViewModel
    {
        [StringLength(50)]
        [RegularExpression(@"^[EeTtXx0-9\.+()-]+$", ErrorMessage = "Home Phone is in incorrect format")]
        [Display(Name = "Home Phone")]
        public string HomePhone { get; set; }
        [StringLength(50)]
        [RegularExpression(@"^[EeTtXx0-9\.+()-]+$", ErrorMessage = "Cell Phone is in incorrect format")]
        [Display(Name = "Cell Phone")]
        public string CellPhone { get; set; }
        [StringLength(50)]
        [RegularExpression(@"^[EeTtX0-9x\.+()-]+$", ErrorMessage = "Business Phone is in incorrect format")]
        [Display(Name = "Business Phone")]
        public string BusinessPhone { get; set; }
        [StringLength(256)]
        [Display(Name = "Email Address")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string EmailAddress { get; set; }
        //[RegularExpression(@"^[0-9\.,]+$", ErrorMessage = "House Size is in incorrect format")]        

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public int CustomerId { get; set; }
    }

    public class ContactAndPaymentInfoViewModel
    {
        public ContactInfoViewModel HomeOwnerContactInfo { get; set; }
        public List<ContactInfoViewModel> CoBorrowersContactInfo { get; set; }
        public PaymentInfoViewModel PaymentInfo { get; set; }
        [Display(Name = "House Size (sq feet)")]
        public double? HouseSize { get; set; }
        public int? ContractId { get; set; }
    }

    public class SummaryAndConfirmationViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        public SendEmailsViewModel SendEmails { get; set; }
        public double ProvinceTaxRate { get; set; }
    }
}
