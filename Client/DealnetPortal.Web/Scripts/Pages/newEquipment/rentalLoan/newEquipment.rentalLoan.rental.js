﻿module.exports('newEquipment.rental', function (require) {
    var tax = require('financial-functions').tax;
    var totalRentalPrice = require('financial-functions').totalRentalPrice;
    var state = require('state').state;

    var settings = {
        totalMonthlyPaymentId: '#total-monthly-payment',
        totalMonthlyPaymentDisplayId: '#total-monthly-payment-display',
        rentalTaxId: '#rentalTax',
        rentalProgramTypeId: '#rental-program-type',
        rentalTotalMonthlyPaymentId: '#rentalTMPayment',
        totalMonthlyPaymentRowId: '#total-monthly-payment-row',
        escalationLimitErrorMsgId: '#escalation-limit-error-msg',
        rentalProgramType: {
            'Escalation0': '0',
            'Escalation35': '1'
        }
    };

    var notNaN = function (num) {
        return !isNaN(num);
    };

    var idToValue = require('idToValue');

    var monthlySum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) {
                return parseFloat(equipment.monthlyCost);
            })
            .filter(notNaN)
            .reduce(function (sum, cost) {
                return sum + cost;
            }, 0);
    };

    /**
     * recalculate all financial values for Rental/RentalHwt agreement type
     * @returns {void} 
     */
    var recalculateAndRenderRentalValues = function () {
        var eSum = monthlySum(state.equipments);

        var data = {
            tax: state.tax,
            equipmentSum: eSum
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        if (notNan && data.equipmentSum !== 0) {
            _onProgramTypeChange($(settings.rentalProgramTypeId).find(":selected").val());

            $(settings.totalMonthlyPaymentId).val(formatNumber(eSum));
            $(settings.totalMonthlyPaymentDisplayId).text(formatNumber(eSum));
            $(settings.rentalTaxId).text(formatNumber(tax(data)));
            $(settings.rentalTotalMonthlyPaymentId).text(formatNumber(totalRentalPrice(data)));
        } else {
            _toggleMonthlyPaymentEscalationErrors(false);
            $(settings.totalMonthlyPaymentId).val('');
            $(settings.totalMonthlyPaymentDisplayId).text('-');
            $(settings.rentalTaxId).text('-');
            $(settings.rentalTotalMonthlyPaymentId).text('-');
        }
    };

    var recalculateRentalTaxAndPrice = function () {
        var data = {
            tax: state.tax,
            equipmentSum: state.rentalMPayment
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        if (notNan && data.equipmentSum !== 0) {
            $(settings.rentalTaxId).text(formatNumber(tax(data)));
            $(settings.rentalTotalMonthlyPaymentId).text(formatNumber(totalRentalPrice(data)));
        } else {
            $(settings.rentalTaxId).text('-');
            $(settings.rentalTotalMonthlyPaymentId).text('-');
        }
    };

    var onProgramTypeChange = function (e) {
        _onProgramTypeChange(e.target.value);
    }

    function _onProgramTypeChange(value) {
        if (state.isStandardRentalTier === true) {
            var eSum = monthlySum(state.equipments);
            var rentalProgramType = value;
            var limit = rentalProgramType === settings.rentalProgramType.Escalation0 ? state.nonEscalatedRentalLimit : state.escalatedRentalLimit;
            _toggleMonthlyPaymentEscalationErrors(!isNaN(eSum) && eSum > limit);
        }
    }

    function _toggleMonthlyPaymentEscalationErrors(show) {
        if (show === true) {
            if ($(settings.escalationLimitErrorMsgId).hasClass('hidden'))
                $(settings.escalationLimitErrorMsgId).removeClass('hidden');
            $(settings.totalMonthlyPaymentRowId).children().each(function () {
                $(this).addClass('error-decorate');
            });

        } else {
            $(settings.escalationLimitErrorMsgId).addClass('hidden');
            $(settings.totalMonthlyPaymentRowId).children().each(function () {
                $(this).removeClass('error-decorate');
            });
        }
    }

    function updateEquipmentSubtypes(parent, type) {
        var select = parent.find('.sub-type-select');
        if (state.isStandardRentalTier && state.agreementType !== 0 && state.equipmentSubTypes[type]) {
            var value = select.val()
            var selected = state.equipmentSubTypes[type].find(function (item) {
                return item.Id == value;
            })
            var subOpt = state.equipmentSubTypes[type].reduce(function (acc, item) {
                return acc.concat($('<option/>', {
                    text: item.Description,
                    value: item.Id
                }));
            }, [$('option', {
                value: '',
                text: ''
            })]);

            select.html(subOpt).removeClass('not-selected');
            select.val(selected ? value : '');
            select.prop('disabled', false);
            select[0].form && select.rules('add', 'required');

            parent.find('.sub-type-col').removeClass('hidden');
            parent.find('.description-col').removeClass('col-md-6').addClass('col-md-3');
        } else {
            select.prop('disabled', true);
            select[0].form && select.rules('remove', 'required');

            parent.find('.sub-type-col').addClass('hidden');
            parent.find('.description-col').addClass('col-md-6').removeClass('col-md-3');
        }
    }

    return {
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice,
        onProgramTypeChange: onProgramTypeChange,
        updateEquipmentSubTypes: updateEquipmentSubtypes
    }
});