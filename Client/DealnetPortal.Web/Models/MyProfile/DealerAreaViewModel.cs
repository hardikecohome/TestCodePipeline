using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class DealerAreaViewModel
    {
        public int Id { get; set; }

        public int ProfileId { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PostalCode")]
        [StringLength(6, MinimumLength = 1, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PostalCodeIncorrectFormat")]
        public string PostalCode { get; set; }
    }
}