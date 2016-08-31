var placeSearch, autocomplete, geocoder;
var addressForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'long_name',
    postal_code: 'short_name'
};
function initGoogleServices() {
    geocoder = new google.maps.Geocoder;

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
    var streetAutocomplete1 = new google.maps.places.Autocomplete(document.getElementById('street'), streetsOptions);
    var cityAutocomplete1 = new google.maps.places.Autocomplete(document.getElementById('locality'), citiesOptions);
    var provenceAutocomplete1 = new google.maps.places.Autocomplete(document.getElementById('administrative_area_level_1'), provencesOptions);
    var streetAutocomplete2 = new google.maps.places.Autocomplete(document.getElementById('mailing_street'), streetsOptions);
    var cityAutocomplete2 = new google.maps.places.Autocomplete(document.getElementById('mailing_locality'), citiesOptions);
    var provenceAutocomplete2 = new google.maps.places.Autocomplete(document.getElementById('mailing_administrative_area_level_1'), provencesOptions);
    google.maps.event.addListener(cityAutocomplete1, 'place_changed', function () {
        var place = cityAutocomplete1.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'locality') {
                document.getElementById('locality').value = place.address_components[i][addressForm[addressType]];
                break;
            }
        }
    });
    google.maps.event.addListener(cityAutocomplete2, 'place_changed', function () {
        var place = cityAutocomplete2.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'locality') {
                document.getElementById('mailing_locality').value = place.address_components[i][addressForm[addressType]];
                break;
            }
        }
    });
    google.maps.event.addListener(provenceAutocomplete1, 'place_changed', function () {
        var place = provenceAutocomplete1.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'administrative_area_level_1') {
                document.getElementById('administrative_area_level_1').value = place.address_components[i][addressForm[addressType]];
                break;
            }
        }
    });
    google.maps.event.addListener(provenceAutocomplete2, 'place_changed', function () {
        var place = provenceAutocomplete2.getPlace();
        for (var i = 0; i < place.address_components.length; i++) {
            var addressType = place.address_components[i].types[0];
            if (addressType == 'administrative_area_level_1') {
                document.getElementById('mailing_administrative_area_level_1').value = place.address_components[i][addressForm[addressType]];
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
            }
        }
        if (street) {
            document.getElementById('street').value = street;
        } else {
            document.getElementById('street').value = '';
        }
    });
    google.maps.event.addListener(streetAutocomplete2, 'place_changed', function () {
        var place = streetAutocomplete2.getPlace();
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
            }
        }
        if (street) {
            document.getElementById('mailing_street').value = street;
        } else {
            document.getElementById('mailing_street').value = '';
        }
    });
}
function autodetectAddress() {
    if (geocoder) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(function (position) {
                var latlng = new google.maps.LatLng(position.coords.latitude, position.coords.longitude);
                geocoder.geocode({
                    'latLng': latlng
                }, function (results, status) {
                    if (status === google.maps.GeocoderStatus.OK) {
                        if (results[0]) {
                            var street;
                            for (var i = 0; i < results[0].address_components.length; i++) {
                                var addressType = results[0].address_components[i].types[0];
                                if (addressForm[addressType]) {
                                    var val = results[0].address_components[i][addressForm[addressType]];
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
                                    document.getElementById(addressType).value = val;
                                }
                            }
                            if (street) {
                                document.getElementById('street').value = street;
                            }
                        } else {
                            console.log('No results by Location service');
                        }
                    } else {
                        console.log('Geocoder failed due to: ' + status);
                    }
                });
            });
        }
    }
}