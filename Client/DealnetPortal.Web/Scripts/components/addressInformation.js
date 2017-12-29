module.exports('addressInformation', function () {
    function AddressInformation (address) {
        this.street = ko.observable(address.Street || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    params: true
                },
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                maxLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 100
                },
                pattern: {
                    message: translations.InstallationAddressIncorrectFormat,
                    params: "[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        this.unit = ko.observable(address.Unit || '')
            .extend({
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 1
                },
                maxLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 10
                },
                pattern: {
                    message: translations.UnitNumberIncorrectFormat,
                    params: '^[a-zA-Z0-9 ]+$'
                }
            });
        this.city = ko.observable(address.City || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    params: true
                },
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                maxLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 50
                },
                pattern: {
                    message: translations.CityIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$"
                }
            });
        this.province = ko.observable(address.Province || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    params: true
                },
                pattern: {
                    message: translations.ProvinceIncorrectFormat,
                    params: "^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$"
                }
            });
        this.postalCode = ko.observable(address.PostalCode || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    params: true
                },
                minLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 5
                },
                maxLength: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 6
                },
                pattern: {
                    message: translations.PostalCodeIncorrectFormat,
                    params: "^[a-zA-Z0-9]+$"
                }
            });

        this.postalCode.subscribe(function (newValue) {
            this.postalCode(newValue.toUpperCase());
        }, this);

        this.isValid = function () {
            return this.street.isValid() &&
                this.unit.isValid() &&
                this.city.isValid() &&
                this.province.isValid() &&
                this.postalCode.isValid();
        }
    };

    AddressInformation.prototype.clearAddress = function () {
        this.installationAddress.street('');
        this.installationAddress.street.isModified(false);
        this.installationAddress.unit('');
        this.installationAddress.unit.isModified(false);
        this.installationAddress.city('');
        this.installationAddress.city.isModified(false);
        this.installationAddress.province('');
        this.installationAddress.province.isModified(false);
        this.installationAddress.postalCode('');
        this.installationAddress.postalCode.isModified(false);
    };

    return AddressInformation;
});
