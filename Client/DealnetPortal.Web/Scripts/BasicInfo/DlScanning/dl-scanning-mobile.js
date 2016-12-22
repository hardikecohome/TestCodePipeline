function submitUpload(sender, uploadUrl, fnToFill, lnToFill, bdToFill, dlToFill, stToFill, ctToFill, prToFill, pcToFill) {
    
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
                        alert(translations['CannotRecognizeDriverLicense']);
                    } else {
                        document.getElementById(fnToFill).value = json.FirstName;
                        document.getElementById(lnToFill).value = json.LastName;
                        //var date = new Date(parseInt(json.DateOfBirth.substr(6)));
                        var date = new Date(json.DateOfBirthStr);
                        date = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate());
                        $("#" + bdToFill).datepicker("setDate", date);
                        document.getElementById(dlToFill).value = json.Id;
                        document.getElementById(stToFill).value = json.Street;
                        document.getElementById(ctToFill).value = json.City;
                        document.getElementById(prToFill).value = json.State;
                        document.getElementById(pcToFill).value = json.PostalCode;
                    }
                },
                error: function (xhr, status, p3) {
                    hideLoader();
                    alert(p3);
                }
            });
            sender.val("");
        } else {
            alert(translations['BrowserNotSupportFileUpload']);
        }
    }
}