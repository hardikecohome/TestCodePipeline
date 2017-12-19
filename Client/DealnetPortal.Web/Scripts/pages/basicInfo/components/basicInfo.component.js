module.exports('basicInfo.component', function (require) {
    var AddressInformation = require('addressInformation');
    var EmploymentInformation = require('employmentInformation');

    return function BasicInfo (model) {
        this.installationAddress = new AddressInformation(model.HomeOwner && model.HomeOwner.AddressInformation ? model.HomeOwner.AddressInformation : {});

        this.showEmployment = ko.computed(function () {
            return this.installationAddress.province() === 'QC';
        }, this);

        this.homeOwner = {
            employment: new EmploymentInformation(model.HomeOwner && model.HomeOwner.EmploymentInformation ? model.HomeOwner.EmploymentInformation : {})
        };

        this.additionalApplicant = {
            employment: new EmploymentInformation(model.AdditionalApplicants[0].EmploymentInformation || {})
        };
    }
});