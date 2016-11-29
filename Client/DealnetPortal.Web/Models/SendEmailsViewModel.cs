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
        [Display(Name = "Additional Applicant Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class SendEmailsViewModel
    {
        public int ContractId { get; set; }

        public int HomeOwnerId { get; set; }

        public AgreementType AgreementType { get; set; }

        [Required]
        public string HomeOwnerFullName { get; set; }

        [Required]
        [Display(Name = "Home Owner Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string HomeOwnerEmail { get; set; }

        [Required]
        [Display(Name = "Sales Rep Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string SalesRepEmail { get; set; }

        public CustomerEmail[] AdditionalApplicantsEmails { get; set; }
    }
}
