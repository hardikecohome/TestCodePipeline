module.exports('new-client-autocomplete', function (require) {
    var concatObj = require('objectUtils').concatObj;
    var compose = require('functionUtils').compose;
    var extendObj = require('objectUtils').extendObj;
    var createAction = require('redux').createAction;
    var clientActions = require('new-client-actions');
    var clientStore = require('new-client-store');

    var dispatch = clientStore.dispatch;

    var setAutocomplete = function (streetElmId, cityElmId) {
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

    var addressForm = {
        street_number: 'short_name',
        route: 'long_name',
        locality: 'long_name',
        administrative_area_level_1: 'short_name',
        country: 'long_name',
        postal_code: 'short_name'
    };

    var getAddress = function (addressForm) {
        return function (addressComponent) {
            var addressType = addressComponent.types[0];
            if (addressForm.hasOwnProperty(addressType)) {
                var addressObj = {};
                addressObj[addressType] = addressComponent[addressForm[addressType]];
                return addressObj;
            }
        };
    };

    var initAutocomplete = function () {
        $(document).ready(function () {
            var gAutoCompletes = setAutocomplete('street', 'locality');
            var gPAutoCompletes = setAutocomplete('pstreet', 'pcity');
            var test = [
                { 'route': clientActions.SET_STREET },
                { 'street_number': clientActions.SET_UNIT },
                { 'locality': clientActions.SET_CITY },
                { 'administrative_area_level_1': clientActions.SET_PROVINCE },
                { 'postal_code': clientActions.SET_POSTAL_CODE }
            ];

            gAutoCompletes.street.addListener('place_changed',
                function () {
                    var place = gAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(clientActions.SET_ADDRESS,
                        {
                            street: place['route'] || '',
                            number: place['street_number'] || '',
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                            postalCode: place['postal_code'] ? place['postal_code'].replace(' ', '') : '',
                        }));
                });

            gAutoCompletes.city.addListener('place_changed',
                function () {
                    var place = gAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(clientActions.SET_ADDRESS,
                        {
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                        }));
                });

            gPAutoCompletes.street.addListener('place_changed',
                function () {
                    var place = gPAutoCompletes.street.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(clientActions.SET_PADDRESS,
                        {
                            street: place['route'] || '',
                            number: place['street_number'] || '',
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                            postalCode: place['postal_code'] ? place['postal_code'].replace(' ', '') : '',
                        }));
                });

            gPAutoCompletes.city.addListener('place_changed',
                function () {
                    var place = gPAutoCompletes.city.getPlace().address_components
                        .map(getAddress(addressForm)).reduce(concatObj);

                    dispatch(createAction(clientActions.SET_PADDRESS,
                        {
                            city: place['locality'] || '',
                            province: place['administrative_area_level_1'] || '',
                        }));
                });
        });
    };
     return {
         initAutocomplete: initAutocomplete
     }
});