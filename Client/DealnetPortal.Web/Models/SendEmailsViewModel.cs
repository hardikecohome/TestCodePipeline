using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;

namespace DealnetPortal.Web.Models
{
    public class CustomerEmail
    {
        public int CustomerId { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AdditionalApplicantEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }

        public string CustomerName { get; set; }
    }

    public class SendEmailsViewModel
    {
        public int ContractId { get; set; }

        public int HomeOwnerId { get; set; }

        public AgreementType AgreementType { get; set; }

        [CustomRequired]
        public string HomeOwnerFullName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "BorrowerEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string BorrowerEmail { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SalesRepEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string SalesRepEmail { get; set; }

        public string SalesRep { get; set; }

        public CustomerEmail[] AdditionalApplicantsEmails { get; set; }
    }
}
