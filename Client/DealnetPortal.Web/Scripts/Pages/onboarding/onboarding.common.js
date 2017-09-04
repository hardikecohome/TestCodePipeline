module.exports('onboarding.common', function () {
    function resetFormValidation(selector) {
        var $form = $(selector);
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse(selector);
    }

    return {
        resetFormValidation: resetFormValidation
    };
});
