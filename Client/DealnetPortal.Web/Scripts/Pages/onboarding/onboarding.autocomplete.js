module.exports('onboarding.autocomplete', function (require) {
    var extendObj = require('objectUtils').extendObj;

    function _setAutocomplete(streetElmId, cityElmId) {
        var streets = [];
        var cities = [];

        var extendCommonOpts = extendObj({
            componentRestrictions: { country: 'ca' }
        });

        var streetElm = document.getElementById(streetElmId);
        var streetAutocomplete = new google.maps.places
            .Autocomplete(streetElm, extendCommonOpts({ types: ['geocode'] }));

        var cityElm = document.getElementById(cityElmId);
        var cityAutocomplete = new google.maps.places
            .Autocomplete(cityElm, extendCommonOpts({ types: ['(cities)'] }));

        return {
            street: streetAutocomplete,
            city: cityAutocomplete
        };
    };

    function _getAddress(addressForm) {
        return function (addressComponent) {
            var addressType = addressComponent.types[0];
            if (addressForm.hasOwnProperty(addressType)) {
                var addressObj = {};
                addressObj[addressType] = addressComponent[addressForm[addressType]];
                return addressObj;
            }
        };
    };

    function addAutocomplete(streetElmId, cityElmId) {
        var autocomplete = _setAutocomplete(streetElmId, cityElmId);

        autocomplete.street.addListener('place_changed',
            function () {
                var place = autocomplete.street.getPlace().address_components
                    .map(_getAddress(addressForm)).reduce(concatObj);

                var postalCode = '';
                if (place['postal_code_prefix']) {
                    postalCode = place['postal_code_prefix'];
                }
                if (place['postal_code']) {
                    postalCode = place['postal_code'].replace(' ', '');
                }

                console.log('street');
                console.log(place['route']);
                console.log(place['street_number']);
                console.log(place['administrative_area_level_1']);
                console.log(postalCode);

                //dispatch(createAction(clientActions.SET_ADDRESS,
                //    {
                //        street: place['route'] || '',
                //        number: place['street_number'] || '',
                //        city: place['locality'] || '',
                //        province: place['administrative_area_level_1'] || '',
                //        postalCode: postalCode
                //    }));
            });

        autocomplete.city.addListener('place_changed',
            function () {
                var place = autocomplete.city.getPlace().address_components
                    .map(_getAddress(addressForm)).reduce(concatObj);

                console.log('street');
                console.log(place['locality']);
                console.log(place['administrative_area_level_1']);
            });
    }

    return {
        add: addAutocomplete
    }
})