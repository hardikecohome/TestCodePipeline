function assignAutocompletes() {
    $(document)
        .ready(function () {
            initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
            initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
        });
}

function submitAsync(url, form, aftervalidateAction) {
    form.validate();
    if (!form.valid()) {
        return;
    };
    if (aftervalidateAction != null) {
        aftervalidateAction();
    }
    $('.sent-email-msg').show();
    $('.send-email-button').text('Resend Emails');
    form.ajaxSubmit({
        type: "POST",
        url: url,
        success: function(json) {
        },
        error: function(xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}