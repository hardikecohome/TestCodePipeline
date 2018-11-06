module.exports('addressInformation', function () {
    function AddressInformation (address) {
        var self = this;
        self.street = ko.observable(address.Street || '')
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
        self.unit = ko.observable(address.Unit || '')
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
        self.city = ko.observable(address.City || '')
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
        self.province = ko.observable(address.Province || '')
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
        self.postalCode = ko.observable(address.PostalCode || '')
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

        self.postalCode.subscribe(function (newValue) {
            self.postalCode(newValue.toUpperCase());
        }, self);

        self.isValid = function () {
            return self.street.isValid() &&
                self.unit.isValid() &&
                self.city.isValid() &&
                self.province.isValid() &&
                self.postalCode.isValid();
        }

        self.clearAddress = function () {
            self.street('');
            self.street.isModified(false);
            self.unit('');
            self.unit.isModified(false);
            self.city('');
            self.city.isModified(false);
            self.province('');
            self.province.isModified(false);
            self.postalCode('');
            self.postalCode.isModified(false);
        };
    };

    return AddressInformation;
});
