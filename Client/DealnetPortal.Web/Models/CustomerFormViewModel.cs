using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class CustomerFormViewModel
    {
        [CheckCustomersAge("AdditionalApplicants", 76)]
        [CheckHomeOwner("AdditionalApplicants")]
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public ContactInfoViewModel HomeOwnerContactInfo { get; set; }
        [Display(Name = "What equipment/service are you interested in?")]
        public string Service { get; set; }
        [Required]
        [Display(Name = "Add Comment")]
        public string Comment { get; set; }
    }
}
