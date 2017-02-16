using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class ApplicantPersonalInfo
    {
        public int? CustomerId { get; set; }

        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "FirstName")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "FirstNameIncorrectFormat")]
        public string FirstName { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "LastName")]
        [StringLength(20, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "LastNameIncorrectFormat")]
        public string LastName { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "BirthDate")]
        [DataType(DataType.Date)]
        [EligibleAge(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "ApplicantNeedsToBeOver18")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? BirthDate { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "SinWithDescription")]
        [StringLength(9, MinimumLength = 9, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "SinMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "SinIncorrectFormat")]
        public string Sin { get; set; } 
        [Display(ResourceType = typeof (Resources.Resources), Name = "DriverLicenseNumber")]
        public string DriverLicenseNumber { get; set; }
        public AddressInformation AddressInformation { get; set; }
        public MailingAddressInformation MailingAddressInformation { get; set; }
        [Display(Name = "Home Owner")]
        public bool IsHomeOwner { get; set; }
        public int? ContractId { get; set; }
    }

    public class MailingAddressInformation : AddressInformation
    {
        [Required]
        [Display(Name = "Street")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "MailingAddressIncorrectFormat")]
        public override string Street { get; set; }
    }

    public class AddressInformation
    {
        [Required]
        [Display(Name = "Street")]
        [StringLength(100, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InstallationAddressIncorrectFormat")]
        public virtual string Street { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "UnitNumber")]
        [StringLength(10, MinimumLength = 1)]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "UnitNumberIncorrectFormat")]
        public string UnitNumber { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "City")]
        [StringLength(50, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CityIncorrectFormat")]
        public string City { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Province")]
        [StringLength(30, MinimumLength = 2)]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "ProvinceIncorrectFormat")]
        public string Province { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "PostalCode")]
        [StringLength(6, MinimumLength = 5)]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "PostalCodeIncorrectFormat")]
        public string PostalCode { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Residence")]
        public Common.Enumeration.ResidenceType ResidenceType { get; set; }
    }

    public class SubDealer    
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
    }
    
    public class BasicInfoViewModel
    {
        public string SubmittingDealerId { get; set; }
        [CheckCustomersAge("AdditionalApplicants", 75)]
        [CheckHomeOwner("AdditionalApplicants")]
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public List<ApplicantPersonalInfo> AdditionalApplicants { get; set; }
        public List<SubDealer> SubDealers { get; set; }
        public int? ContractId { get; set; }
        public ContractState ContractState { get; set; }
    }

    public class PaymentInfoViewModel
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "PaymentType")]
        public Common.Enumeration.PaymentType PaymentType { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "PrefferedWithdrawalDateIncorrectFormat")]
        public Common.Enumeration.WithdrawalDateType PrefferedWithdrawalDate { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "BankNumberIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "BlankNumber")]
        public string BlankNumber { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9 ]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "TransitNumberIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "TransitNumber")]
        public string TransitNumber { get; set; }
        [StringLength(20)]
        [RegularExpression(@"^[0-9- ]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "AccountNumberIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AccountNumber")]
        public string AccountNumber { get; set; }
        [StringLength(12, MinimumLength = 12, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "EnbridgeGasDistributionAccountMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "EnbridgeGasDistributionAccountIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "EnbridgeGasDistributionAccount")]
        public string EnbridgeGasDistributionAccount { get; set; }
        [StringLength(7)]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "MeterNumberIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "MeterNumber")]
        public string MeterNumber { get; set; }
    }

    public class ContactInfoViewModel
    {
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "HomePhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "HomePhoneIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "HomePhone")]
        public string HomePhone { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CellPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CellPhoneIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "CellPhone")]
        public string CellPhone { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "BusinessPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "BusinessPhoneIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "BusinessPhone")]
        public string BusinessPhone { get; set; }
        [StringLength(256)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool AllowCommunicate { get; set; }

        public int CustomerId { get; set; }
    }

    public class ContactAndPaymentInfoViewModel
    {
        public ContactInfoViewModel HomeOwnerContactInfo { get; set; }
        public List<ContactInfoViewModel> CoBorrowersContactInfo { get; set; }
        public PaymentInfoViewModel PaymentInfo { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }
        public int? ContractId { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }
    }

    public class AdditionalInfoViewModel
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "Status")]
        public Common.Enumeration.ContractState ContractState { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "Status")]
        public string Status { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "Date")]
        public DateTime? LastUpdateTime { get; set; }
        public string TransactionId { get; set; }
    }

    public class SummaryAndConfirmationViewModel
    {
        public BasicInfoViewModel BasicInfo { get; set; }
        public EquipmentInformationViewModel EquipmentInfo { get; set; }
        public ContactAndPaymentInfoViewModel ContactAndPaymentInfo { get; set; }
        public AdditionalInfoViewModel AdditionalInfo { get; set; }
        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }
        public LoanCalculator.Output LoanCalculatorOutput { get; set; }
    }

    public sealed class EligibleAgeAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            var date = (DateTime)value;
            var todayDate = DateTime.Today;
            //In case of 29 February we make it 1 March (because date with 29 February minus 18 years equals date with 28 February)
            return date <= (todayDate.Day == 29 ? todayDate.AddDays(1).AddYears(-18) : todayDate.AddYears(-18));
        }
    }
}
