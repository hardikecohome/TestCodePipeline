using DealnetPortal.Api.Common.Enumeration;
using System;

namespace DealnetPortal.Web.Models
{
    public class SigneeViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public DateTime? StatusLastUpdateTime { get; set; }
        public SignatureRole? Role { get; set; }
        public SignatureStatus? SignatureStatus { get; set; }
    }
}