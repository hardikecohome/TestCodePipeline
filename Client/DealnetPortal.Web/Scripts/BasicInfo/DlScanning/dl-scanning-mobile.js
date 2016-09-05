function submitUpload(sender, uploadUrl, fnToFill, lnToFill, bdToFill) {
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
                    }
                },
                error: function (xhr, status, p3) {
                    alert(xhr.responseText);
                }
            });
            sender.val("");
        } else {
            alert("Browser doesn't support HTML5 file upload!");
        }
    }
}