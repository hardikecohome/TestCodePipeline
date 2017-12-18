module.exports('employmentInformationVM', function (require) {
    var addressInformation = require('addressInformation');
    return function employmentInformationVM (info) {
        return {
            status: ko.observable(info.EmploymentStatus || ''),
            incomeType: ko.observable(info.IncomType || ''),
            annualSalary: ko.observable(info.AnnualSalary || ''),
            hourlyRate: ko.observable(info.HourlyRate || ''),
            yearsOfEmploy: ko.observable(info.YearsOfEmployment || ''),
            monthsOfEmploy: ko.observable(info.MonthsOfEmploy || ''),
            type: ko.observable(info.EmploymentType || ''),
            jobTitle: ko.observable(info.JobTitle || ''),
            companyName: ko.observable(info.CompanyName || ''),
            companyPhone: ko.observable(info.CompanyPhone || ''),
            address: ko.observable(addressInformation(info.CompanyAddress))
        };
    };
});
