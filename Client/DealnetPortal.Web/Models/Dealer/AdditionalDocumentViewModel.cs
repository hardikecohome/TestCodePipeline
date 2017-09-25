using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class AdditionalDocumentViewModel
    {
        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        public string Number { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool NotExpired { get; set; }

        public int LicenseTypeId { get; set; }
    }
}