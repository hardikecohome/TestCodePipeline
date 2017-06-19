function setValidationRelation(elem1, elem2) {
    elem1.change(function () {        
        if ($(this).val()) {
            elem2.rules("remove", "required");
            elem2.removeClass('input-validation-error');
            elem2.next('.text-danger').empty();
        } else {
            elem2.rules("add", "required");
        }
    });
}