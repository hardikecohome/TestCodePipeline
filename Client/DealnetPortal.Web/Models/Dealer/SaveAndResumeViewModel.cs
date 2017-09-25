using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class SaveAndResumeViewModel
    {

        public bool Success { get; set; }

        public IEnumerable<Alert> Alerts { get; set; }

        public int Id { get; set; }

        public string AccessKey { get; set; }

        [CustomRequired]
        public bool AllowCommunicate { get; set; }

        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }

        public bool InvalidFields { get; set; }
    }
}