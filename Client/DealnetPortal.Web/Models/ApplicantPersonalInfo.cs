using System;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class ApplicantPersonalInfo
    {
        public int? CustomerId { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "FirstName")]
        [StringLength(30, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "FirstNameIncorrectFormat")]
        public string FirstName { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "LastName")]
        [StringLength(30, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LastNameIncorrectFormat")]
        public string LastName { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "BirthDate")]
        [DataType(DataType.Date)]
        [EligibleAge(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ApplicantNeedsToBeOver18")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? BirthDate { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "SinWithDescription")]
        [StringLength(9, MinimumLength = 9, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SinMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SinIncorrectFormat")]
        public string Sin { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "DriverLicenseNumber")]
        public string DriverLicenseNumber { get; set; }
        public AddressInformation AddressInformation { get; set; }
        public AddressInformation MailingAddressInformation { get; set; }
        public AddressInformation PreviousAddressInformation { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "HomeOwner")]
        public bool IsHomeOwner { get; set; }
        public bool? IsInitialCustomer { get; set; }
        public int? ContractId { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "VerificationId")]
        public string VerificationIdName { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "DealerInitial")]
        [StringLength(2, MinimumLength = 1, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "DealerInitialIncorrectFormat")]
        public string DealerInitial { get; set; }

        public EmploymentInformationViewModel EmploymentInformation { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "RelationshipToMainBorrower")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RelationshipToMainBorrowerIncorrectFormat")]
        public string RelationshipToMainBorrower { get; set; }
    }
}
