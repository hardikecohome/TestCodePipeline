﻿configInitialized
    .then(function () {
        var setValidationRelation = module.require('setValidationRelation');
        var settings = {
            homePhoneClass: '.home-phone',
            cellPhoneClass: '.cell-phone',
            enbridgeClass: '.enbridge-payment',
            papClass: '.pap-payment',
            paymentInfoAvailableId: '#payment-info-available',
            paymentTypeId: '#payment-type',
            paymentTypes: {
                'PAP': '0',
                'ENBRIDGE': '1'
            },
            enbridgeFeeNotificationId: '#enbridge-fee-notification'
        }

        $(settings.homePhoneClass).each(function () {
            $(this).rules("add", "required");
        });

        $(settings.cellPhoneClass).each(function () {
            $(this).rules("add", "required");
        });

        $('.mandatory-phones').each(function () {
            var homePhone = $(this).find(settings.homePhoneClass);
            var cellPhone = $(this).find(settings.cellPhoneClass);

            if (homePhone.length && cellPhone.length) {
                setValidationRelation(homePhone, cellPhone);
                setValidationRelation(cellPhone, homePhone);
            }
        });

        $(settings.homePhoneClass).change();
        $(settings.cellPhoneClass).change();

        var isAllPaymentInfoAvailable = $(settings.paymentInfoAvailableId).val().toLowerCase() === 'true';
        var $paymentTypeSelector = $(settings.paymentTypeId);

        var initPaymentType = isAllPaymentInfoAvailable ? $paymentTypeSelector.find(":selected").val() : settings.paymentTypes.PAP;
        managePaymentElements(initPaymentType);

        if (!isAllPaymentInfoAvailable) {
            $paymentTypeSelector.val(settings.paymentTypes.PAP);
            $paymentTypeSelector.attr('readonly', true);
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

        function managePaymentElements(paymentType) {
            var $papPaymentSelector = $(settings.papClass);
            var $enbridgePaymentSelector = $(settings.enbridgeClass);
            var $enbridgeFeeNotification = $(settings.enbridgeFeeNotificationId);
            switch (paymentType) {
                case settings.paymentTypes.ENBRIDGE:
                    $papPaymentSelector.hide();
                    $papPaymentSelector.find('input, select').each(function () {
                        $(this).prop("disabled", true);
                    });
                    $enbridgePaymentSelector.show();
                    $enbridgePaymentSelector.find('input, select').each(function () {
                        $(this).prop("disabled", false);
                        $(this).rules('add', 'required');
                    });
                    $enbridgeFeeNotification.show();
                    break;
                case settings.paymentTypes.PAP:
                    $enbridgePaymentSelector.hide();
                    $enbridgePaymentSelector.find('input, select').each(function () {
                        $(this).prop("disabled", true);
                        $(this).rules('remove', 'required');
                    });
                    $enbridgeFeeNotification.hide();
                    $papPaymentSelector.show();
                    $papPaymentSelector.find('input, select').each(function () {
                        $(this).prop("disabled", false);
                    });
                    break;
            }
        }
    });