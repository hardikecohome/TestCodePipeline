function submitUpload(sender, uploadUrl, fnToFill, lnToFill, bdToFill, fillAddress) {
    var files = sender.files;
    if (files.length > 0) {
        if (window.FormData !== undefined) {
            var data = new FormData();
            for (var x = 0; x < files.length; x++) {
                data.append("file" + x, files[x]);
            }

            $.ajax({
                type: "POST",
                url: uploadUrl,
                contentType: false,
                processData: false,
                data: data,
                success: function (json) {
                    if (json.isError) {
                        alert("Can't recognize driver license");
                    } else {
                        document.getElementById(fnToFill).value = json.FirstName;
                        document.getElementById(lnToFill).value = json.LastName;
                        var date = new Date(parseInt(json.DateOfBirth.substr(6)));
                        $("#" + bdToFill).datepicker("setDate", date);
                        if (fillAddress) {
                            document.getElementById('street').value = json.Street;
                            document.getElementById('locality').value = json.City;
                            document.getElementById('administrative_area_level_1').value = json.State;
                            document.getElementById('postal_code').value = json.PostalCode;
                        }
                    }
                },
                error: function (xhr, status, p3) {
                    alert(p3);
                }
            });
            sender.val("");
        } else {
            alert("Browser doesn't support HTML5 file upload!");
        }
    }
}