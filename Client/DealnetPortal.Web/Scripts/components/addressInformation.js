module.exports('addressInformation', function () {
    function AddressInformation (address) {
        this.street = ko.observable(address.Street || '');
        this.unit = ko.observable(address.Unit || '');
        this.city = ko.observable(address.City || '');
        this.province = ko.observable(address.Province || '');
        this.postalCode = ko.observable(address.PostalCode || '');

        this.postalCode.subscribe(function (newValue) {
            this.postalCode(newValue.toUppperCase());
        }, this);
    };

    AddressInformation.prototype.clearAddress = function () {
        this.street('');
        this.unit('');
        this.city('');
        this.province('');
        this.postalCode('');
    };

    return addressInformation;
});
