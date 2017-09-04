using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class AdditionalDocumentViewModel
    {
        public string Number { get; set; }

        public DateTime ExpiredDate { get; set; }

        public int LicenseTypeId { get; set; }
    }
}