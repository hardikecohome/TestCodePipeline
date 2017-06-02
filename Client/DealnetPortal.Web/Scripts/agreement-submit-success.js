$(function() {
    togglePrintButton(checkUrl);
    $('#print-button').on('click', printContract(downloadUrl));
});

function submitEmailsAsync(url, form) {
    form.validate();
    if (!form.valid()) {
        return;
    };
    $('.sent-email-msg').show();
    $("#send-email-button").text(translations['ResendEmails']);
    form.ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
        },
        error: function (xhr, status, p3) {
        }
    });
}