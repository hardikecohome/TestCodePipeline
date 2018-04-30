﻿module.exports('newEquipment.rental', function (require) {
    var tax = require('financial-functions').tax;
    var totalRentalPrice = require('financial-functions').totalRentalPrice;
    var state = require('state').state;

    var settings = {
        totalMonthlyPaymentId: '#total-monthly-payment',
        totalMonthlyPaymentDisplayId: '#total-monthly-payment-display',
        rentalTaxId: '#rentalTax',
        rentalTotalMonthlyPaymentId: '#rentalTMPayment',
        totalMonthlyPaymentRowId: '#total-monthly-payment-row',
        escalationLimitErrorMsgId: '#escalation-limit-error-msg',
        rentalProgramType: {
            'Escalation0': '0',
            'Escalation35': '1'                
        }
    }

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

            if (state.isStandardRentalTier === true) {
                var rentalProgramType = $(settings.rentalProgramTypeId).find(":selected").val();
                var limit = rentalProgramType === settings.rentalProgramType.Escalation0 ? state.nonEscalatedRentalLimit : state.escalatedRentalLimit;
                _toggleMonthlyPaymentEscalationErrors(!isNaN(eSum) && eSum >= limit);
            }

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

    function _toggleMonthlyPaymentEscalationErrors(show) {
        if (show === true) {
            if ($(settings.escalationLimitErrorMsgId).hasClass('hidden'))
                $(settings.escalationLimitErrorMsgId).removeClass('hidden');
            $(settings.totalMonthlyPaymentRowId).children().each(function() {
                $(this).addClass('error-decorate');
            });

        } else {
            $(settings.escalationLimitErrorMsgId).addClass('hidden');
            $(settings.totalMonthlyPaymentRowId).children().each(function () {
                $(this).removeClass('error-decorate');
            });
        }
    }

    return {
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice
    }
});