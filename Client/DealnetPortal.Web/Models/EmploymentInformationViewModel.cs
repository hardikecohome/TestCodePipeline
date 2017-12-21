using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class EmploymentInformationViewModel
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmploymentStatus")]
        public EmploymentStatus? EmploymentStatus { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "IncomeType")]
        public IncomeType? IncomeType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        public string AnnualSalary { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        public string HourlyRate { get; set; }

        public string YearsOfEmployment { get; set; }

        public string MonthsOfEmployment { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="TypeOfEmployment")]
        public EmploymentType? EmploymentType { get; set; }

        [Display(ResourceType =typeof(Resources.Resources), Name ="JobTitle")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CityIncorrectFormat")]
        public string JobTitle { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="CompanyName")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "JobTitleIncorrectFormat")]
        public string CompanyName { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneIncorrectFormat")]
        public string CompanyPhone { get; set; }

        public AddressInformation CompanyAddress { get; set; }
    }
}