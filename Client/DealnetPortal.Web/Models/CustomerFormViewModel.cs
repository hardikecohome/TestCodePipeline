using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Models.Validation;

namespace DealnetPortal.Web.Models
{
    public class CustomerFormViewModel
    {
        [CheckHomeOwner("AdditionalApplicants")]
        public ApplicantPersonalInfo HomeOwner { get; set; }
        public CustomerContactInfoViewModel HomeOwnerContactInfo { get; set; }
        [Display(ResourceType = typeof (Resources.Resources), Name = "WhatEquipmentServiceYouInterestedIn")]
        public string Service { get; set; }
        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CannotBeLongerThan")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AddComment")]
        public string Comment { get; set; }
        [Required]
        public string DealerName { get; set; }

        public string HashDealerName { get; set; }

        public bool IsQuebecDealer { get; set; }
    }
}
