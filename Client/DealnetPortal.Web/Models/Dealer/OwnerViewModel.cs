using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class OwnerViewModel
    {
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "FirstName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "FirstNameIncorrectFormat")]
        public string FirstName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "LastName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LastNameIncorrectFormat")]
        public string LastName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "BirthDate")]
        [DataType(DataType.Date)]
        [EligibleAge(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ApplicantNeedsToBeOver18")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? BirthDate { get; set; }

        public AddressInformation Address { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "HomePhone")]
        public string HomePhone { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MobilePhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MobilePhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "MobilePhone")]
        public string CellPhone { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public virtual string EmailAddress { get; set; }

        [Range(1,100, ErrorMessageResourceType =typeof(Resources.Resources), ErrorMessageResourceName = "PercentOwnershipRange")]
        [Display(ResourceType =typeof(Resources.Resources), Name = "PercentOwnership")]
        public double? PercentOwnership { get; set; }
    }
}