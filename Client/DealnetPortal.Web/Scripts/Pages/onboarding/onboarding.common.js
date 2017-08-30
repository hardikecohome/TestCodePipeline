module.exports('onboarding.common', function () {
    function resetFormValidation() {
        var $form = $('form');
        $form.removeData("validator");
        $form.removeData("unobtrusiveValidation");
        $.validator.unobtrusive.parse('form');
    }

    return {
        resetFormValidation: resetFormValidation
    };
});
