configInitialized
    .then(function() {
        $("#check-credit-button").click(function(event) {

            if (!$('#home-owner-agrees').is(':checked') || !$('#agreement-checkbox-data2').is(':checked')) {
                event.preventDefault();
                $("#proceed-error-message").show();
            }

            if ($('#additional1-agrees').length && $('#consent-checkbox-data1').length) {
                if (!$('#additional1-agrees').is(':checked') || !$('#consent-checkbox-data1').is(':checked')) {
                    event.preventDefault();
                    $("#proceed-error-message").show();
                }
            }
        });
    });