module.exports('employmentInformation', function (require) {
    var addressInformation = require('addressInformation');
    return function EmploymentInformationVM (info) {
        var self = this;
        self.status = ko.observable(info.EmploymentStatus || '');
        self.incomeType = ko.observable(info.IncomeType || '').extend({
            required: {
                message: translations['ThisFieldIsRequired'],
                onlyIf: function () {
                    return self.status() === '0';
                }
            }
        });
        self.annualSalary = ko.observable(info.AnnualSalary || '').extend({
            required: {
                message: translations.ThisFieldIsRequired,
                onlyIf: function () {
                    return self.status() !== '0' || self.incomeType() === '0';
                }
            }
        });
        self.hourlyRate = ko.observable(info.HourlyRate || '').extend({
            required: {
                message: translations.ThisFieldIsRequired,
                onlyIf: function () {
                    return self.status() === '0' && self.incomeType() === '1';
                }
            }
        });
        self.yearsOfEmploy = ko.observable(info.YearsOfEmployment || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        var stat = self.status();
                        return stat === '0' || stat === '2';
                    }
                }
            })
        self.monthsOfEmploy = ko.observable(info.MonthsOfEmploy || '');
        self.type = ko.observable(info.EmploymentType || '');
        self.jobTitle = ko.observable(info.JobTitle || '');
        self.companyName = ko.observable(info.CompanyName || '');
        self.companyPhone = ko.observable(info.CompanyPhone || '');
        self.address = ko.validatedObservable(new addressInformation(info.CompanyAddress || {}));

        self.isEmployed = ko.computed(function () {
            return this.status() === '0';
        }, self);

        self.showAnnualSalary = ko.computed(function () {
            return this.status() !== '0' || this.incomeType() === '0';
        }, self);

        self.showHourlyRate = ko.computed(function () {
            return this.status() === '0' && this.incomeType() === '1';
        }, self);

        self.isEmployedOrSelfEmployed = ko.computed(function () {
            return this.isEmployed() || this.status() === '2';
        }, self);
    };
});
