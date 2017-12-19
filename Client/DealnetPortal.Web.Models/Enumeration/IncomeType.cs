using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum IncomeType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        Annual,
        [Display(ResourceType = typeof(Resources.Resources), Name = "Hourly")]
        Hourly
    }
}
