using System;

namespace DealnetPortal.Web.Models
{
    public class SigneeViewModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Comment { get; set; }
        public DateTime? StatusLastUpdateTime { get; set; }
    }
}