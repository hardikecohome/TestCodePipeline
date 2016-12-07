var placeSearch, autocomplete, geocoder;
var addressForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'long_name',
    postal_code: 'short_name',
    postal_code_prefix: 'short_name'
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
                $('#locality').removeClass('pac-placeholder').removeClass('placeholder');
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
                $('#mailing_locality').removeClass('pac-placeholder').removeClass('placeholder');
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
                $('#administrative_area_level_1').removeClass('pac-placeholder').removeClass('placeholder');
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
                $('#mailing_administrative_area_level_1').removeClass('pac-placeholder').removeClass('placeholder');
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
                    document.getElementById('locality').value = val;
                    $('#locality').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
                if (addressType == 'administrative_area_level_1') {
                    document.getElementById('administrative_area_level_1').value = val;
                    $('#administrative_area_level_1').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
                if (addressType == 'postal_code' || addressType == 'postal_code_prefix') {
                    document.getElementById('postal_code').value = val;
                    $('#postal_code').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
            }
        }
        var streetInput = $('#street');
        if (street) {
            streetInput.val(street);
            streetInput.removeClass('pac-placeholder').removeClass('placeholder');
        } else {
            streetInput.val('');
            streetInput.placeholder();
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
                if (addressType == 'locality') {
                    document.getElementById('mailing_locality').value = val;
                    $('#mailing_locality').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
                if (addressType == 'administrative_area_level_1') {
                    document.getElementById('mailing_administrative_area_level_1').value = val;
                    $('#mailing_administrative_area_level_1').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
                if (addressType == 'postal_code' || addressType == 'postal_code_prefix') {
                    document.getElementById('mailing_postal_code').value = val;
                    $('#mailing_postal_code').removeClass('pac-placeholder').removeClass('placeholder');
                    continue;
                }
            }
        }
        var streetInput = $('#mailing_street');
        if (street) {
            streetInput.val(street);
            streetInput.removeClass('pac-placeholder').removeClass('placeholder');
        } else {
            streetInput.val('');
            streetInput.placeholder();
        }
    });

    for (var j = 1; j <= 3; j++) {
        var streetAutocomplete = new google.maps.places.Autocomplete(document.getElementById('additional-street-' + j), streetsOptions);
        var cityAutocomplete = new google.maps.places.Autocomplete(document.getElementById('additional-locality-' + j), citiesOptions);
        var provenceAutocomplete = new google.maps.places.Autocomplete(document.getElementById('additional-administrative_area_level_1-' + j), provencesOptions);
        google.maps.event.addListener(cityAutocomplete, 'place_changed', function (cityAutocomplete, j) {
            var place = cityAutocomplete.getPlace();
            for (var i = 0; i < place.address_components.length; i++) {
                var addressType = place.address_components[i].types[0];
                if (addressType == 'locality') {
                    document.getElementById('additional-locality-' + j).value = place.address_components[i][addressForm[addressType]];
                    $('#additional-locality-' + j).removeClass('pac-placeholder').removeClass('placeholder');
                    break;
                }
            }
        }.bind(this, cityAutocomplete, j));
        google.maps.event.addListener(provenceAutocomplete, 'place_changed', function (provenceAutocomplete, j) {
            var place = provenceAutocomplete.getPlace();
            for (var i = 0; i < place.address_components.length; i++) {
                var addressType = place.address_components[i].types[0];
                if (addressType == 'administrative_area_level_1') {
                    document.getElementById('additional-administrative_area_level_1-' + j).value = place.address_components[i][addressForm[addressType]];
                    $('#additional-administrative_area_level_1-' + j).removeClass('pac-placeholder').removeClass('placeholder');
                    break;
                }
            }
        }.bind(this, provenceAutocomplete, j));
        google.maps.event.addListener(streetAutocomplete, 'place_changed', function (streetAutocomplete, j) {
            var place = streetAutocomplete.getPlace();
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
                        var localityInput = $('#additional-locality-' + j);
                        localityInput.val(val);
                        localityInput.removeClass('pac-placeholder').removeClass('placeholder');
                        continue;
                    }
                    if (addressType == 'administrative_area_level_1') {
                        var admAreaInput = $('#additional-administrative_area_level_1-' + j);
                        admAreaInput.val(val);
                        admAreaInput.removeClass('pac-placeholder').removeClass('placeholder');
                        continue;
                    }
                    if (addressType == 'postal_code' || addressType == 'postal_code_prefix') {
                        var postalCodeInput = $('#additional-postal_code-' + j);
                        postalCodeInput.val(val);
                        postalCodeInput.removeClass('pac-placeholder').removeClass('placeholder');
                        continue;
                    }
                }
            }
            var streetInput = $('#additional-street-' + j);
            if (street) {
                streetInput.val(street);
                streetInput.removeClass('pac-placeholder').removeClass('placeholder');
            } else {
                streetInput.val('');
                streetInput.placeholder();
            }
        }.bind(this, streetAutocomplete, j));
    }
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
                        //$('#street, #locality, #administrative_area_level_1').removeClass('placeholder').removeClass('pac-placeholder');
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
                                    if (addressType == 'postal_code_prefix') {
                                        document.getElementById('postal_code').value = val;
                                        continue;
                                    }

                                    document.getElementById(addressType).value = val;
                                    $('#'+addressType).removeClass('placeholder').removeClass('pac-placeholder');
                                }
                            }
                            if (street) {
                                $('#street').removeClass('placeholder').removeClass('pac-placeholder');
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