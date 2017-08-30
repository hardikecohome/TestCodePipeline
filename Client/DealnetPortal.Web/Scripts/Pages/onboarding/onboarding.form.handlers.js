module.exports('onboarding.form.handlers', function (require) {
    var state = require('onboarding.state').state;

    return function (e) {
        var equipValid = validateEquipment();
        var workProvinceValid = validateWorkProvinces();

        if (equipValid && workProvinceValid) {
            if ($('form').valid()) {
                showLoader();
                $('form').ajaxSubmit({
                type:'POST',

                });
            } else {
                e.preventDefault();
            }
        } else {

        }
    };

    function validateEquipment() {
        if (state.selectedEquipment.length > 1) {
            $('#equipment-error').text(translations.SelectOneProduct);
            return false;
        } else {
            $('#equipment-error').text('');
        }
        return true;
    }

    function validateWorkProvinces() {
        if (state.selectedProvinces.length > 1) {
            $('#work-province-error').text(translations.SelectOneProvince);
            return false;
        } else {
            $('#work-province-error').text('');
        }
        return true;
    }

    function successCallback(json) {
        hideLoader();
    }

    function errorCallback(xhr, status, p3) {
        hideLoader();
    }
});
