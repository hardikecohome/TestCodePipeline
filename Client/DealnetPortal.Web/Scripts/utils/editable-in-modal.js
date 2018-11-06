function copyFormData (form1, form2, validate, additionalValidation) {
    if (validate) {
        form1.validate();
        if (!form1.valid() || (typeof additionalValidation == 'function' && !additionalValidation())) {
            return false;
        };
    }

    $(':input[name]', form2).val(function () {
        var sourceInput = $(':input[name=\'' + this.name + '\']', form1);
        if (!sourceInput.length || sourceInput.valid() != true) {
            //console.log(this.value);
            return this.value;
        }
        return sourceInput.val();
    });
    $('.text-danger span', form2).remove();
    $(':input[name]', form2).removeClass('input-validation-error');

    setTimeout(function () {
        $('input, select, textarea', form2).each(function () {
            var inpValue = $(this).is('select') ? $(this).find("option:selected").text() : $(this).val();
            $(this).parents(".dealnet-field-holder").find('.dealnet-disabled-input-value').html(inpValue.replace(/\r?\n/g, '<br />'));
        });
    }, 300);

    return true;
}

function saveChanges (form1, form2, url, mainform, afterCopy, additionalValidation) {

    if (copyFormData(form1, form2, true, additionalValidation)) {
        if (typeof afterCopy == 'function') { afterCopy(); }
        submitChanges(url, mainform);
        return true;
    }
    return false;
};

function submitChanges (url, mainform) {
    module.require('loader').showLoader();
    mainform.ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
            module.require('loader').hideLoader();
            if (json.isError) {
                alert(translations['ErrorWhileUpdatingData']);
            }
        },
        error: function (xhr, status, p3) {
            module.require('loader').hideLoader();
            alert(xhr.responseText);
        }
    });
}