function checkPrintedContract(checkUrl, downloadUrl) {
    $.ajax({
        type: "POST",
        url: checkUrl,
        success: function (json) {
            if (json == true) {
                $('#print-error-message').hide();
                window.location = downloadUrl;
            } else {
                $('#print-error-message').show();
            }
        },
        error: function (xhr, status, p3) {
            $('#print-error-message').show();
        }
    });
}