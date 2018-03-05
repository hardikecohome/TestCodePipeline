using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models
{
    public class RecaptchaViewModel
    {
        [Required]
        public string Response { get; set; }
    }
}