function uploadCaptured(uploadUrl) {
    var dataUrl = bigCanvas.toDataURL();
    showLoader(translations['ProcessingImage']);
    $.ajax({
        type: "POST",
        url: uploadUrl,
        data: {
            imgBase64: dataUrl
        },
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert(translations['CannotRecognizeVoidCheque']);
            } else {
                document.getElementById('bank-number').value = json.BankNumber;
                document.getElementById('transit-number').value = json.TransitNumber;
                document.getElementById('account-number').value = json.AccountNumber;
                $('#camera-modal').modal('hide');
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}

function submitUpload(sender, uploadUrl) {

    var files = sender.files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }
            showLoader(translations['ProcessingImage']);
            $.ajax({
                type: "POST",
                url: uploadUrl,
                contentType: false,
                processData: false,
                data: data,
                success: function (json) {
                    hideLoader();
                    if (json.isError) {
                        alert(translations['CannotRecognizeVoidCheque']);
                    } else {
                        document.getElementById('bank-number').value = json.BankNumber;
                        document.getElementById('transit-number').value = json.TransitNumber;
                        document.getElementById('account-number').value = json.AccountNumber;
                        $('#camera-modal').modal('hide');
                    }
                },
                error: function (xhr, status, p3) {
                    hideLoader();
                    alert(p3);
                }
            });
            $("#upload-file").val("");
        } else {
            alert(translations['BrowserNotSupportFileUpload']);
        }
    }
}