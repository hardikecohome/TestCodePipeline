using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.Enumeration
{
    public enum IncomeType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        AnnualSalaryIncome = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        HourlyRate = 1
    }
}
