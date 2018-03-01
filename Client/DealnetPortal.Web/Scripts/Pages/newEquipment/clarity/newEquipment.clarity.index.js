module.exports('newEquipment.clarity.index', function (require) {
    var state = require('state').state;
    var calrityUi = require('newEquipment.clarity.ui');
    var equipment = require('equipment');
    var packages = require('newEquipment.clarity.packages');
    var calculate = require('newEquipment.clairty.calculation').recalculateClarityValuesAndRender;
    var setters = require('value-setters');

    var settings = {
        isNewContractId: '#IsNewContract',
        agreementTypeId: '#typeOfAgreementSelect',
        submitButtonId: '#submit',
        downPaymentId: '#downPayment',
        addEquipmentId: '#addEquipment',
        addExistingEquipmentId: '#addExistingEqipment',
        totalAmountFinancedId: '#total-amount-financed',
        equipmentValidationMessageId: '#new-equipment-validation-message',
        loanRateCardToggleId: '#loanRateCardToggle',
        addInstallationPackageId: '#addInstallationPackage',
        formId: '#equipment-form'
    }

    var init = function (id, cards) {
        state.contractId = id;
        // check if we have any prefilled values in database
        // related to this contract, if yes contract is not new
        var agreementType = $(settings.agreementTypeId).find(":selected").val();
        state.agreementType = Number(agreementType);
        state.isNewContract = $(settings.isNewContractId).val().toLowerCase() === 'true';
        state.clarity = cards[0];
        state.isClarity = true;

        setters.init({
            isClarity: true,
            recalculateClarityValuesAndRender: calculate
        });
        equipment.init({
            isClarity: true,
            recalculateClarityValuesAndRender: calculate
        });
        packages.init({
            isClarity: true,
            recalculateClarityValuesAndRender: calculate
        });

        _initHandlers();
        calrityUi.init();

        if (!state.isNewContract) {
            $(settings.loanRateCardToggleId).click();
            calculate();
        }
    }

    function _initHandlers() {
        $(settings.submitButtonId).on('click', _submitForm);
        $(settings.downPaymentId).on('change', setters.setDownPayment);
        $(settings.totalMonthlyPaymentId).on('change', setters.setRentalMPayment);
        $(settings.addEquipmentId).on('click', equipment.addEquipment);
        $(settings.addExistingEquipmentId).on('click', equipment.addExistingEquipment);
        $(settings.addInstallationPackageId).on('click', packages.addPackage);
    }

    function _submitForm(event) {
        $(settings.formId).valid();
        var monthPayment = Globalize.parseNumber($(settings.totalAmountFinancedId).text().replace('$', '').trim());

        if (isNaN(monthPayment) || monthPayment <= 0) {
            event.preventDefault();
            $(settings.equipmentValidationMessageId).text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
            return;
        }

        $('#AmortizationTerm').val(state['clarity'].AmortizationTerm);
        $('#LoanTerm').val(state['clarity'].LoanTerm);
        $('#new-clarity-contract').val(true);
        $('#total-monthly-payment').val($('#totalMonthlyCostTax').text().substring(1));
        $('#CustomerRate').val(formatNumber(state['clarity'].CustomerRate));
        $('#AdminFee').val(formatNumber(state['clarity'].AdminFee));
        $('#SelectedRateCardId').val(state['clarity'].Id);

        $(settings.formId).submit();
    }

    return {
        init: init
    }
})