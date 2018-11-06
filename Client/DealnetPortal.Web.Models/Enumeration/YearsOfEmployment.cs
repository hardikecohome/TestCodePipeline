using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum YearsOfEmployment
    {
        [Display(Name = "0")] Zero,
        [Display(Name = "1")] One,
        [Display(Name = "2")] Two,
        [Display(Name = "3")] Three,
        [Display(Name = "4")] Four,
        [Display(Name = "5")] Five,
        [Display(Name = "6")] Six,
        [Display(Name = "7")] Seven,
        [Display(Name = "8")] Eight,
        [Display(Name = "9")] Nine,
        [Display(Name = "10+")] TenPlus
    }
}
