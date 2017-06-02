function printContract(downloadUrl) {
    var url = downloadUrl;

    return function(e) {
        window.location = url;
    }
}

function togglePrintButton(checkUrl) {
    $.ajax({
        type: "POST",
        url: checkUrl,
        success: function (json) {
            if (json == true) {
                if($('#print-button').is('.disabled'))
                {
                    $('#print-button').removeClass('disabled');
                }
            } else {
                if (!$('#print-button').is('.disabled')) {
                    $('#print-button').addClass('disabled'); 
                }
            }
        },
        error: function (xhr, status, p3) {
            $('#print-error-message').show();
        }
    });
}