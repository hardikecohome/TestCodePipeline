configInitialized
    .then(function () {
        var setValidationRelation = module.require('setValidationRelation');
        var settings = {
            homePhoneClass: '.home-phone',
            cellPhoneClass: '.cell-phone',
            gasAccountId: '#enbridge-gas-distribution-account',
            meterNumberId: '#meter-number',
            paymentInfoAvailableId: '#payment-info-available',
            paymentTypeId: '#payment-type',
            paymentTypes: {
                'ENBRIDGE': '0',
                'PAP': '1'
            }
        }

        $(settings.homePhoneClass).each(function () {
            $(this).rules("add", "required");
        });

        $(settings.cellPhoneClass).each(function () {
            $(this).rules("add", "required");
        });
        var $gasAccountSelector = $(settings.gasAccountId);
        var $meterNumberSelector = $(settings.meterNumberId);

        $gasAccountSelector.rules("add", "required");
        $meterNumberSelector.rules("add", "required");

        $('.mandatory-phones').each(function () {
            var homePhone = $(this).find(settings.homePhoneClass);
            var cellPhone = $(this).find(settings.cellPhoneClass);

            if (homePhone.length && cellPhone.length) {
                setValidationRelation(homePhone, cellPhone);
                setValidationRelation(cellPhone, homePhone);
            }
        });
        setValidationRelation($gasAccountSelector, $meterNumberSelector);
        setValidationRelation($meterNumberSelector, $gasAccountSelector);
        $(settings.homePhoneClass).change();
        $(settings.cellPhoneClass).change();
        $gasAccountSelector.change();
        $meterNumberSelector.change();

        var isAllPaymentInfoAvailable = $(settings.paymentInfoAvailableId).val().toLowerCase() === 'true';
        var $paymentTypeSelector = $(settings.paymentTypeId);

        var initPaymentType = isAllPaymentInfoAvailable ? $paymentTypeSelector.find(":selected").val() : settings.paymentTypes.PAP;
        managePaymentElements(initPaymentType);

        if (!isAllPaymentInfoAvailable) {
            $paymentTypeSelector.val(settings.paymentTypes.PAP);
            $paymentTypeSelector.attr('disabled', true);
        }

        $paymentTypeSelector.change(function () {
            managePaymentElements($(this).find(":selected").val());
        });

        var navigateToStep = module.require('navigateToStep');
        $('#steps .step-item[data-warning="true"]').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });

        $('form').on('submit', function (e) {
            if (!$('form').valid()) {
                e.preventDefault();
            }

        });
    });

function managePaymentElements(paymentType) {
    var $papPaymentSelector = $(".pap-payment");
    var $enbridgePaymentSelector = $(".enbridge-payment");
    switch (paymentType) {
        case '0':
            $papPaymentSelector.hide();
            $papPaymentSelector.find('input, select').each(function () {
                $(this).prop("disabled", true);
            });
            $enbridgePaymentSelector.show();
            $enbridgePaymentSelector.find('input, select').each(function () {
                $(this).prop("disabled", false);
            });
            break;
        case '1':
            $enbridgePaymentSelector.hide();
            $enbridgePaymentSelector.find('input, select').each(function () {
                $(this).prop("disabled", true);
            });
            $papPaymentSelector.show();
            $papPaymentSelector.find('input, select').each(function () {
                $(this).prop("disabled", false);
            });
            break;
    }
}