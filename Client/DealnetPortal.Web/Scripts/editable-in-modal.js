function copyFormData(form1, form2, validate) {
    if (validate) {
        form1.validate();
        if (!form1.valid()) {
            return false;
        };
    }
    $(':input[name]', form2).val(function () {
        return $(':input[name=\'' + this.name + '\']', form1).val();
    });
    $('.text-danger span', form2).remove();
    $(':input[name]', form2).removeClass('input-validation-error');
    return true;
}

function saveChanges(form1, form2, url, mainform) {
    if (copyFormData(form1, form2, true)) {
        submitChanges(url, mainform);
        return true;
    }
    return false;
};

function submitChanges(url, mainform) {
    showLoader();
    mainform.ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while updating data");
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}