using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models
{
    public class HelpPopUpViewModal
    {
        public int Id { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "DealerName")]
        public string DealerName { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "YourName")]
        public string YourName { get; set; }
        public bool IsPreferedContactPerson { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredContactPerson")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NameIncorrectFormat")]
        public string PreferedContactPerson { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNumber")]
        [StringLength(10, MinimumLength = 4, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ContractNumberIncorrectFormat")]
        public string LoanNumber { get; set; }

        public SupportTypeEnum SupportType { get; set; }

        [CustomRequired]
        public string HelpRequested { get; set; }
        public BestWayEnum BestWay { get; set; }

        //[StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneMustBeLong")]
        //[RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneIncorrectFormat")]
        //[Display(ResourceType = typeof(Resources.Resources), Name = "HomePhone")]


        //[StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        //[Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        //[EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]

        [CustomRequired]
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        public string Phone { get; set; }
        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }
    }

    public enum SupportTypeEnum
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "CreditDecision")]
        creditFunding = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "CreditDocs")]
        customerService = 1,
        [Display(ResourceType = typeof(Resources.Resources), Name = "FundingDocs")]
        dealerSupport = 2,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Other")]
        Other = 3
    }

    public enum BestWayEnum
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        Phone = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Email")]
        Email = 1
    }
}