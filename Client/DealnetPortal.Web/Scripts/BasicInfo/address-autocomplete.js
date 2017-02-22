var geocoder;
var addressForm = {
    street_number: 'short_name',
    route: 'long_name',
    locality: 'long_name',
    administrative_area_level_1: 'long_name',
    postal_code: 'short_name',
    postal_code_prefix: 'short_name'
};
function assignAutocompletes() {
    geocoder = new google.maps.Geocoder;

    initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
    initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
    initGoogleServices("previous_street", "previous_locality", "previous_administrative_area_level_1", "previous_postal_code");
    for (var i = 1; i <= 3; i++) {
        initGoogleServices("additional-street-" + i, "additional-locality-" + i, "additional-administrative_area_level_1-" + i, "additional-postal_code-" + i);
        initGoogleServices("additional-previous-street-" + i, "additional-previous-locality-" + i, "additional-previous-administrative_area_level_1-" + i, "additional-previous-postal_code-" + i);
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
                                    if (addressType == 'postal_code' || addressType == 'postal_code_prefix') {
                                        val = val.replace(/\s+/g, '');
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