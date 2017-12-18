using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum IncomeType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Annual")]
        Annual,
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalaryIncome")]
        SalaryIncome,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Hourly")]
        Hourly
    }
}
