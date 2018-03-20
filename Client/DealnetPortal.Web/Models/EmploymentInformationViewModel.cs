using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class EmploymentInformationViewModel
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "MonthlyMortgagePayment")]
        [RegularExpression(@"^[1-9]\d{0,4}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MonthlyMortgagePaymentIncorrectFormat")]
        public double MonthlyMortgagePayment { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "EmploymentStatus")]
        public EmploymentStatus EmploymentStatus { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "IncomeType")]
        public IncomeType? IncomeType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        [RegularExpression(@"^[1-9]\d{0,5}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AnnualSalaryIncomeIncorrectFormat")]
        public string AnnualSalary { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        [RegularExpression(@"^[1-9]\d{0,2}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HourlyRateIncorrectFormat")]
        public string HourlyRate { get; set; }

        public string YearsOfEmployment { get; set; }

        public string MonthsOfEmployment { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="TypeOfEmployment")]
        public EmploymentType? EmploymentType { get; set; }

        [Display(ResourceType =typeof(Resources.Resources), Name ="JobTitle")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string JobTitle { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="CompanyName")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string CompanyName { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name="CompanyPhone")]
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneIncorrectFormat")]
        public string CompanyPhone { get; set; }

        public AddressInformation CompanyAddress { get; set; }
    }
}