using System;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models
{
    public class SignerViewModel
    {
        public int Id { get; set; }
        public int? CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        [CustomRequired]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }
        public string Comment { get; set; }
        public DateTime? StatusLastUpdateTime { get; set; }
        [CustomRequired]
        public SignatureRole? Role { get; set; }
        public SignatureStatus? SignatureStatus { get; set; }
    }
}