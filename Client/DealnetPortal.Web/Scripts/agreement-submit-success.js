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

function checkPrintedContract(checkUrl, downloadUrl) {
    $.ajax({
        type: "POST",
        url: checkUrl,
        success: function (json) {
            if (json == true) {
                window.location = downloadUrl;
            } else {
                $('.danger-well').show();
            }
        },
        error: function (xhr, status, p3) {
            $('.danger-well').show();
        }
    });
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
}