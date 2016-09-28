function uploadCaptured(uploadUrl) {
    var dataUrl = bigCanvas.toDataURL();
    showLoader();
    $.ajax({
        type: "POST",
        url: uploadUrl,
        data: {
            imgBase64: dataUrl
        },
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("Can't recognize driver license");
            } else {
                var modal = document.getElementById('camera-modal');
                document.getElementById(modal.getAttribute('data-fnToFill')).value = json.FirstName;
                document.getElementById(modal.getAttribute('data-lnToFill')).value = json.LastName;
                var date = new Date(parseInt(json.DateOfBirth.substr(6)));
                document.getElementById(modal.getAttribute('data-bdToFill')).value = date;
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
            showLoader();
            $.ajax({
                type: "POST",
                url: uploadUrl,
                contentType: false,
                processData: false,
                data: data,
                success: function (json) {
                    hideLoader();
                    if (json.isError) {
                        alert("Can't recognize driver license");
                    } else {
                        var modal = document.getElementById('camera-modal');
                        document.getElementById(modal.getAttribute('data-fnToFill')).value = json.FirstName;
                        document.getElementById(modal.getAttribute('data-lnToFill')).value = json.LastName;
                        var date = new Date(parseInt(json.DateOfBirth.substr(6)));
                        $("#" + modal.getAttribute('data-bdToFill')).datepicker("setDate", date);
                        var fillAddress = modal.getAttribute('data-fillAddress');
                        if (fillAddress == "true") {
                            document.getElementById('street').value = json.Street;
                            document.getElementById('locality').value = json.City;
                            document.getElementById('administrative_area_level_1').value = json.State;
                            document.getElementById('postal_code').value = json.PostalCode;
                        }
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
            alert("Browser doesn't support HTML5 file upload!");
        }
    }
}