﻿configInitialized
    .then(function() {
        $("#check-credit-button").click(function(event) {
			ga('send', 'event', 'Credit check confirmation', 'button_click', 'Step 2 from Dealer Portal', '100');
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