module.exports('employmentInformation', function (require) {
    var addressInformation = require('addressInformation');
    return function EmploymentInformationVM (info) {
        var self = this;

        self.status = ko.observable(info.EmploymentStatus || '');

        self.incomeType = ko.observable(info.IncomeType || '')
            .extend({
                required: {
                    message: translations['ThisFieldIsRequired'],
                    onlyIf: function () {
                        return self.status() == '0';
                    }
                }
            });

        self.isEmployed = ko.computed(function () {
            return this.status() == '0';
        }, self);

        self.showAnnualSalary = ko.computed(function () {
            return this.status() != '0' || this.incomeType() == '0';
        }, self);

        self.showHourlyRate = ko.computed(function () {
            return this.status() == '0' && this.incomeType() == '1';
        }, self);

        self.isEmployedOrSelfEmployed = ko.computed(function () {
            return this.isEmployed() || this.status() == '2';
        }, self);

        self.annualSalary = ko.observable(info.AnnualSalary || '')
            .extend({
                required: {
                    onlyIf: function () {
                        return self.showAnnualSalary();
                    }
                }
            });
        self.hourlyRate = ko.observable(info.HourlyRate || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.showHourlyRate();
                    }
                }
            });
        self.yearsOfEmploy = ko.observable(info.YearsOfEmployment || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed();
                    }
                }
            });
        self.monthsOfEmploy = ko.observable(info.MonthsOfEmploy || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed() && self.yearsOfEmploy() < 10;
                    }
                }
            });
        self.type = ko.observable(info.EmploymentType || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployed();
                    }
                }
            });
        self.jobTitle = ko.observable(info.JobTitle || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed();
                    }
                },
                min: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                max: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 140
                },
                pattern: {
                    message: translations.JobTitleIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        self.companyName = ko.observable(info.CompanyName || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed();
                    },
                    min: {
                        message: translations.TheFieldMustBeMinimumAndMaximum,
                        params: 2
                    },
                    max: {
                        message: translations.TheFieldMustBeMinimumAndMaximum,
                        params: 140
                    },
                    pattern: {
                        message: translations.CompanyNameIncorrectFormat,
                        params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                    }
                }
            });
        self.companyPhone = ko.observable(info.CompanyPhone || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed();
                    }
                },
                min: {
                    message: translations.CompanyPhoneMustBeLong,
                    params: 10
                },
                max: {
                    message: translations.CompanyPhoneMustBeLong,
                    params: 10
                },
                pattern: {
                    message: translations.CityIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        self.address = ko.validatedObservable(new addressInformation(info.CompanyAddress || {}))
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.isEmployedOrSelfEmployed();
                    }
                }
            });
    };
});
