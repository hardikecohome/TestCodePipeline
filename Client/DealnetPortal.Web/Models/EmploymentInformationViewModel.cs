using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class EmploymentInformationViewModel
    {
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmploymentStatus")]
        public EmploymentStatus EmploymentStatus { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "IncomeType")]
        public IncomeType? IncomeType { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalaryIncome")]
        public string AnnualSalary { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "HourlyRate")]
        public string HourlyRate { get; set; }
        public YearsOfEmployment YearsOfEmployment { get; set; }
        public MonthsOfEmployment MonthsOfEmployment { get; set; }
        public EmploymentType? EmploymentType { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string CompanyPhone { get; set; }
        public AddressInformation Address { get; set; }
    }
}