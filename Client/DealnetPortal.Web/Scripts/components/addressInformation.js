module.exports('addressInformation', function () {
    function AddressInformation (address) {
        this.street = ko.observable(address.Street || '')
            .extend({
                required: {
                    message: translations.ThisFieldIsRequired,
                    params: true
                },
                min: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                max: {
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
                min: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 1
                },
                max: {
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
                min: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 2
                },
                max: {
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
                min: {
                    message: translations.TheFieldMustBeMinimumAndMaximum,
                    params: 5
                },
                max: {
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
    };

    AddressInformation.prototype.clearAddress = function () {
        this.street('');
        this.unit('');
        this.city('');
        this.province('');
        this.postalCode('');
    };

    return AddressInformation;
});
