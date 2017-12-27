describe('addressInformationTests', function () {
     translations = {
        ThisFieldIsRequired: '',
        TheFieldMustBeMinimumAndMaximum: '',
        InstallationAddressIncorrectFormat: '',
        UnitNumberIncorrectFormat: '',
        CityIncorrectFormat: '',
        ProvinceIncorrectFormat: '',
        PostalCodeIncorrectFormat:''
    };
    var AddressInformation =module.require('addressInformation');

    it('should initialize', function () {
        var address = new AddressInformation({});

        expect(address).toBeDefined();
    });
});
