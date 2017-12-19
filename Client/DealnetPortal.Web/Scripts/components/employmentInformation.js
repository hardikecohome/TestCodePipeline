module.exports('employmentInformation', function (require) {
    var addressInformation = require('addressInformation');
    return function EmploymentInformationVM (info) {
        this.status = ko.observable(info.EmploymentStatus || '');
        this.incomeType = ko.observable(info.IncomeType || '');
        this.annualSalary = ko.observable(info.AnnualSalary || '');
        this.hourlyRate = ko.observable(info.HourlyRate || '');
        this.yearsOfEmploy = ko.observable(info.YearsOfEmployment || '');
        this.monthsOfEmploy = ko.observable(info.MonthsOfEmploy || '');
        this.type = ko.observable(info.EmploymentType || '');
        this.jobTitle = ko.observable(info.JobTitle || '');
        this.companyName = ko.observable(info.CompanyName || '');
        this.companyPhone = ko.observable(info.CompanyPhone || '');
        this.address = ko.observable(new addressInformation(info.CompanyAddress || {}));

        this.showIncomeType = ko.computed(function () {
            return this.status().toLowerCase() === 'employed';
        }, this);

        this.showAnnualSalary = ko.computed(function () {
            return this.incomeType().toLowerCase() === 'annual';
        }, this);

        this.showHourlyRate = ko.computed(function () {
            return this.incomeType().toLowerCase() === 'hourly';
        }, this);
    };
});
