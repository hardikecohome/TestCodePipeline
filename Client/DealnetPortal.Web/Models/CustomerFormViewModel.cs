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
        [Display(ResourceType = typeof (Resources.Resources), Name = "WhatEquipmentServiceYouInterestedIn")]
        public string Service { get; set; }
        [Required]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AddComment")]
        public string Comment { get; set; }
    }
}
