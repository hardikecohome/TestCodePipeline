using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Api.Common.Enumeration.Employment
{
    public enum IncomeType
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "Annual")]
        AnnualSalaryIncome = 0,
        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        HourlyRate = 1
    }
}
