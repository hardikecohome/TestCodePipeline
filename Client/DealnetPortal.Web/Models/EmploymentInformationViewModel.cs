using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class EmploymentInformationViewModel
    {
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmploymentStatus")]
        public EmploymentStatus? EmploymentStatus { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "IncomeType")]
        public IncomeType? IncomeType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        public string AnnualSalary { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        public string HourlyRate { get; set; }

        public YearsOfEmployment? YearsOfEmployment { get; set; }

        public MonthsOfEmployment? MonthsOfEmployment { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="TypeOfEmployment")]
        public EmploymentType? EmploymentType { get; set; }

        [Display(ResourceType =typeof(Resources.Resources), Name ="JobTitle")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string JobTitle { get; set; }

        [Display(ResourceType =typeof(Resources.Resources),Name ="CompanyName")]
        [StringLength(140, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string CompanyName { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CompanyPhoneIncorrectFormat")]
        public string CompanyPhone { get; set; }

        public AddressInformation CompanyAddress { get; set; }
    }
}