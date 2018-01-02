module.exports('employmentInformation', function (require) {
    var AddressInformation = require('addressInformation');
    return function EmploymentInformationVM (info) {
        var self = this;
        
        self.status = ko.observable(info.EmploymentStatus || '0');

        self.incomeType = ko.observable(info.IncomeType == undefined ? '' : info.IncomeType.toString())
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
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
            var stat = this.status();
            return stat == '0' || stat == '2';
        }, self);

        self.annualSalary = ko.observable(info.AnnualSalary || '')
            .extend({
                required: {
                    onlyIf: function() {
                        return self.showAnnualSalary();
                    }
                }
            });
        self.hourlyRate = ko.observable(info.HourlyRate||'')
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
                    onlyIf: function() {
                        return self.isEmployedOrSelfEmployed();
                    }
                }
            });

        self.enableMonths = ko.computed(function() {
            return self.isEmployedOrSelfEmployed() && self.yearsOfEmploy() !== '10+';
        });

        self.monthsOfEmploy = ko.observable(info.MonthsOfEmployment||'')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function () {
                        return self.enableMonths();
                    }
                }
            });
        self.type = ko.observable(info.EmploymentType == undefined ? '' : info.EmploymentType.toString())
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function() {
                        return self.isEmployed();
                    }
                }
            });
        self.jobTitle = ko.observable(info.JobTitle || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function() {
                        return self.isEmployedOrSelfEmployed();
                    }
                },
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                maxLength: {
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
                    onlyIf: function() {
                        return self.isEmployedOrSelfEmployed();
                    }
                },
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                maxLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 140
                },
                pattern: {
                    message: translations.CompanyNameIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        self.companyPhone = ko.observable(info.CompanyPhone || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    onlyIf: function() {
                        return self.isEmployedOrSelfEmployed();
                    }
                },
                minLength: {
                    message: translations.CompanyPhoneMustBeLong,
                    params: 10
                },
                maxLength: {
                    message: translations.CompanyPhoneMustBeLong,
                    params: 10
                },
                pattern: {
                    message: translations.CompanyPhoneIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        self.address = new AddressInformation(info.CompanyAddress || {});

        self.yearsOfEmploy.subscribe(function (newValue) {
            if (newValue === '10+') {
                this.monthsOfEmploy('');
            }
        }, self);

        self.valid = function () {
            var addressValid = self.isEmployedOrSelfEmployed() ? self.address.isValid() : true;
            return self.status() != '' &&
                self.incomeType.isValid() &&
                self.annualSalary.isValid() &&
                self.hourlyRate.isValid() &&
                self.yearsOfEmploy.isValid() &&
                self.monthsOfEmploy.isValid() &&
                self.type.isValid() &&
                self.jobTitle.isValid() &&
                self.companyName.isValid() &&
                self.companyPhone.isValid() &&
                addressValid;
        };

        self.showAllMessages = function () {
            ko.validation.group(self, { deep: true }).showAllMessages(true);
        }
    };
});
