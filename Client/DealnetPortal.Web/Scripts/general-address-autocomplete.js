var addressForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'long_name',
    postal_code: 'short_name',
    postal_code_prefix: 'short_name'
};
function initGoogleServices(streetId, cityId, provenceId, postalCodeId) {
    var streetInput = document.getElementById(streetId);
    var cityInput = document.getElementById(cityId);
    var provenceInput = document.getElementById(provenceId);
    var postalCodeInput = document.getElementById(postalCodeId);
    if (streetInput == null || cityInput == null || provenceInput == null || postalCodeInput == null) { return; }
    var streetsOptions = {
        types: ['geocode'],
        componentRestrictions: { country: "ca" }
    };
    var citiesOptions = {
        types: ['(cities)'],
        componentRestrictions: { country: "ca" }
    };
    var provencesOptions = {
        types: ['(regions)'],
        componentRestrictions: { country: "ca" }
    };
    var streetAutocomplete1 = new google.maps.places.Autocomplete(streetInput, streetsOptions);
    var cityAutocomplete1 = new google.maps.places.Autocomplete(cityInput, citiesOptions);
    var provenceAutocomplete1 = new google.maps.places.Autocomplete(provenceInput, provencesOptions);
    google.maps.event.addListener(cityAutocomplete1, 'place_changed', function () {
        var place = cityAutocomplete1.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'locality') {
                cityInput.value = place.address_components[i][addressForm[addressType]];
                break;
            }
        }
    });
    google.maps.event.addListener(provenceAutocomplete1, 'place_changed', function () {
        var place = provenceAutocomplete1.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'administrative_area_level_1') {
                provenceInput.value = place.address_components[i][addressForm[addressType]];
                break;
            }
        }
    });
    google.maps.event.addListener(streetAutocomplete1, 'place_changed', function () {
        var place = streetAutocomplete1.getPlace();
        var street;
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressForm[addressType]) {
                var val = place.address_components[i][addressForm[addressType]];
                if (addressType == 'street_number') {
                    if (!street) {
                        street = val;
                    } else {
                        street = val + " " + street;
                    }
                    continue;
                }
                if (addressType == 'route') {
                    if (!street) {
                        street = val;
                    } else {
                        street += " " + val;
                    }
                    continue;
                }
                if (addressType == 'locality') {
                    cityInput.value = val;
                    continue;
                }
                if (addressType == 'administrative_area_level_1') {
                    provenceInput.value = val;
                    continue;
                }
                if (addressType == 'postal_code' || addressType == 'postal_code_prefix') {
                    postalCodeInput.value = val.replace(/\s+/g, '');
                    continue;
                }
            }
        }
        if (street) {
            streetInput.value = street;
        } else {
            streetInput.value = '';
        }
    });
}