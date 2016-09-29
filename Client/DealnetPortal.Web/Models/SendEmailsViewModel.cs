using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Models
{
    public class AdiitionalApplicantEmail
    {
        [Display(Name = "Adiitional Applicant Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
    }

    public class SendEmailsViewModel
    {
        public int ContractId { get; set; }

        [Display(Name = "Home Owner Email")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string HomeOwnerEmail { get; set; }

        public AdiitionalApplicantEmail[] AdditionalApplicantsEmails { get; set; }
    }
}
