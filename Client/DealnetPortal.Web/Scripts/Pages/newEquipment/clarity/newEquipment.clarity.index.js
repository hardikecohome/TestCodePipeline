module.exports('newEquipment.clarity.index', function(require) {
    var state = require('state').state;
    var calrityUi = require('newEquipment.clarity.ui');
    var equipment = require('equipment');
    var calculate = require('newEquipment.clairty.calculation').recalculateClarityValuesAndRender;
    var setters = require('value-setters');
    var settings = {
        isNewContractId: '#IsNewContract',
        agreementTypeId: '#typeOfAgreementSelect',
        submitButtonId: '#submit',
        downPaymentId: '#downPayment',
        addEquipmentId: '#addEquipment',
        addExistingEquipmentId: '#addExistingEqipment'
    }

    var init = function(id, cards) {
        state.contractId = id;
        // check if we have any prefilled values in database
        // related to this contract, if yes contract is not new
        var agreementType = $(settings.agreementTypeId).find(":selected").val();
        state.agreementType = Number(agreementType);
        state.isNewContract = $(settings.isNewContractId).val().toLowerCase() === 'true';
        state.clarity = cards[0];

        setters.init({ isClarity: true, recalculateClarityValuesAndRender: calculate });
        equipment.init({isClarity: true, recalculateClarityValuesAndRender : calculate});

        _initHandlers();
        calrityUi.init();
    }

    function _initHandlers () {
        $(settings.submitButtonId).on('click', _submitForm);
        $(settings.downPaymentId).on('change', setters.setDownPayment);
        $(settings.totalMonthlyPaymentId).on('change', setters.setRentalMPayment);
        $(settings.addEquipmentId).on('click', equipment.addEquipment);
        $(settings.addExistingEquipmentId).on('click', equipment.addExistingEquipment);
    }

    function _submitForm (event) {
        $(settings.formId).valid();
        var agreementType = $(settings.agreementTypeId).find(":selected").val();
        var monthPayment = agreementType === settings.applicationType.loanApplication ?
            Globalize.parseNumber($('#' + option + 'TMPayments').text().replace('$', '').trim()) :
            Globalize.parseNumber($(settings.rentalMonthlyPaymentId).text());

        if (isNaN(monthPayment) || monthPayment <= 0) {
            event.preventDefault();
            $(settings.equipmentValidationMessageId).text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
            return;
        }

        $(settings.formId).submit();
    }


    return { init: init }
})