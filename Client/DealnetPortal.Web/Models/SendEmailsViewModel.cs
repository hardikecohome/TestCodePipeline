using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class CustomerEmail
    {
        public int CustomerId { get; set; }

        //[Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AdditionalApplicantEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }
    }

    public class SendEmailsViewModel
    {
        public int ContractId { get; set; }

        public int HomeOwnerId { get; set; }

        public Common.Enumeration.AgreementType AgreementType { get; set; }

        [Required]
        public string HomeOwnerFullName { get; set; }

        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "HomeOwnerEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string HomeOwnerEmail { get; set; }

        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SalesRepEmail")]
        [EmailAddress(ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string SalesRepEmail { get; set; }

        public CustomerEmail[] AdditionalApplicantsEmails { get; set; }
    }
}
