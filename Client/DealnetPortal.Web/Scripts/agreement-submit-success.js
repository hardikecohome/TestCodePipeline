function submitEmailsAsync(url, form) {
    form.validate();
    if (!form.valid()) {
        return;
    };
    $('.sent-email-msg').show();
    $("#send-email-button").text('Resend Emails');
    form.ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
        },
        error: function (xhr, status, p3) {
            alert(xhr.responseText);
        }
    });
}

//$('.sent-email-msg').show();
    //$("#send-email-button").text('Resend Emails');
    //form.ajaxSubmit({
    //    type: "POST",
    //    url: url,
    //    success: function (json) {
    //    },
    //    error: function (xhr, status, p3) {
    //        alert(xhr.responseText);
    //    }
    //});