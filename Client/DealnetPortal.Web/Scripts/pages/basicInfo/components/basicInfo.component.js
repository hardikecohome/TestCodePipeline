module.exports('basicInfo.component', function (require) {
    var AddressInformation = require('addressInformation');
    var EmploymentInformation = require('employmentInformation');

    return function BasicInfo (model) {
        this.installationAddress = new AddressInformation(model.HomeOwner &&
            model.HomeOwner.AddressInformation ?
            model.HomeOwner.AddressInformation : {});

        this.showEmployment = ko.computed(function () {
            return this.installationAddress.province() === 'QC';
        }, this);

        this.homeOwner = {
            employment: new EmploymentInformation(model.HomeOwner &&
                model.HomeOwner.EmploymentInformation ?
                model.HomeOwner.EmploymentInformation : {})
        };

        this.hasAdditional = ko.observable(model.AdditionalApplicants &&
            model.AdditionalApplicants[0].FirstName ? true : false);

        this.additionalApplicant = {
            employment: new EmploymentInformation(model.AdditionalApplicants && model.AdditionalApplicants[0].EmploymentInformation || {})
        };

        this.allowQcDealerProceed = function() {
            if(!model.QuebecDealer) return true;

            return this.showEmployment();
        }

        this.valid = function () {
            var valid = true;
            if (this.showEmployment()) {
                valid = this.homeOwner.employment.valid();
                if (this.hasAdditional()) {
                    valid = this.additionalApplicant.employment.valid() && valid;
                }
            }
            if (!valid) {
                this.homeOwner.employment.showAllMessages();
                if (this.hasAdditional()) {
                    this.additionalApplicant.employment.showAllMessages();
                }
            }
            
            return valid;
        }
    }
});