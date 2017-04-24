module.exports('rate-cards', function (require) {

    var totalObligation = require('financial-functions').totalObligation;
    var totalPrice = require('financial-functions').totalPrice;
    var totalAmountFinanced = require('financial-functions').totalAmountFinanced;
    var monthlyPayment = require('financial-functions').monthlyPayment;
    var totalMonthlyPayments = require('financial-functions').totalMonthlyPayments;
    var residualBalance = require('financial-functions').residualBalance;
    var totalBorrowingCost = require('financial-functions').totalBorrowingCost;
    var yourCost = require('financial-functions').yourCost;
    var tax = require('financial-functions').tax;

    var contractId;
    var rateCards = [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }, { id: 3, name: 'Custom' }];
    var numberFields = ['equipmentSum', 'loanTerm', 'amortTerm', 'customerRate', 'adminFee'];
    var notCero = ['equipmentSum', 'loanTerm', 'amortTerm'];

    window.state = {
        agreementType: 0,
        equipments: {
            '0': {
                type: '',
                description: '',
                cost: 0,
                template: ''
            }
        },
        tax: 12,
        downPayment: 0,
        rentalMPayment: 0,
        Custom: {
            loanTerm: '',
            amortTerm: '',
            deferralPeriod: '',
            customerRate: '',
            yourCost: '',
            adminFee: ''
        }
    };

    var notNaN = function (num) { return !isNaN(num); };

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var renderTotalPrice = function (data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan) {
            $('#totalEquipmentPrice').text(formatNumber(data.equipmentSum));
            $('#tax').text(formatNumber(data.tax));
            $('#totalPrice').text(formatNumber(data.totalPrice));
        } else {
            $('#totalEquipmentPrice').text('-');
            $('#tax').text('-');
            $('#totalPrice').text('-');
        }
    };

    var setBasicValues = function () {
        rateCards.forEach(function (option) {
            var items = $.parseJSON(sessionStorage.getItem(contractId + option.name));
            var formatted = +$('#' + option.name + 'AmortizationDropdown').val();
            var amortization = $.grep(items, function (i) {
                return i.AmortizationTerm === formatted;
            })[0];

            if (amortization !== null && amortization !== undefined) {

                state[option.name] = amortization;
                state[option.name].yourCost = '';

                $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
                $('#' + option.name + 'CRate').text(state[option.name].CustomerRate + ' %');
                $('#' + option.name + 'YCostVal').text(state[option.name].DealerCost + ' %');
            }
        });
    }

    var renderOption = function (option, data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = numberFields.every(function (field) {
            return typeof data[field] === 'number';
        });

        var validateNotEmpty = notCero.every(function (field) {
            return data[field] !== 0;
        });

       // if (notNan && validateNumber && validateNotEmpty) {
        if (validateNotEmpty) {
            $('#' + option + 'MPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + 'CBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + 'TAFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + 'TMPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + 'RBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + 'TObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + 'YCost').text(formatCurrency(data.yourCost));
        } else {
            $('#' + option + 'MPayment').text('-');
            $('#' + option + 'CBorrowing').text('-');
            $('#' + option + 'TAFinanced').text('-');
            $('#' + option + 'TMPayments').text('-');
            $('#' + option + 'RBalance').text('-');
            $('#' + option + 'TObligation').text('-');
            $('#' + option + 'YCost').text('-');
        }
    };

    var recalculateValuesAndRender = function (options) {
        setBasicValues();

        var optionsToCompute = options || rateCards;

        var eSum = equipmentSum(state.equipments);

        renderTotalPrice({
            equipmentSum: eSum !== 0 ? eSum : '-',
            tax: eSum !== 0 ? tax({ equipmentSum: eSum, tax: state.tax }) : '-',
            totalPrice: eSum !== 0 ? totalPrice({ equipmentSum: eSum, tax: state.tax }) : '-'
        });

        optionsToCompute.forEach(function (option) {
            var data = $.extend({}, idToValue(state)(option.name),
                {
                    equipmentSum: eSum,
                    downPayment: state.downPayment,
                    tax: state.tax
                });

            renderOption(option.name, $.extend({}, data, {
                monthlyPayment: monthlyPayment(data),
                costOfBorrowing: totalBorrowingCost(data),
                totalAmountFinanced: totalAmountFinanced(data),
                totalMonthlyPayments: totalMonthlyPayments(data),
                residualBalance: residualBalance(data),
                totalObligation: totalObligation(data),
                yourCost: yourCost(data)
            }));
        });
    };

    var recalculateAndRenderRentalValues = function () {
        var eSum = equipmentSum(state.equipments);

        var data = {
            tax: state.tax,
            equipmentSum: eSum,
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#rentalMPayment').val(eSum);
            $('#rentalTax').text(tax(data));
            $('#rentalTMPayment').text(totalPrice(data));
        } else {
            $('#rentalMPayment').val('');
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };

    var recalculateRentalTaxAndPrice = function () {
        var data = {
            tax: state.tax,
            equipmentSum: state.rentalMPayment,
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#rentalTax').text(tax(data));
            $('#rentalTMPayment').text(totalPrice(data));
        } else {
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };
    function setHandlers(option) {
        $('#' + option.name + 'AmortizationDropdown').change(function() {
            recalculateValuesAndRender();
        });
    }
    var initializeRateCards = function (id, targetUrl) {
        contractId = id;
        var cards = sessionStorage.getItem(contractId);
        if (cards === null) {
            $.ajax({
                type: 'GET',
                url: targetUrl,
                cache: true,
                success: function (result) {
                    rateCards.forEach(function(option) {
                        var filtred = $.grep(result, function(v) {
                            return v.CardType === option.id;
                        });
                        sessionStorage.setItem(contractId + option.name, JSON.stringify(filtred));
                        setHandlers(option);
                    });
                    recalculateValuesAndRender();
                },
                error: function () {
                    alert('error');
                }
            });
        }
    }

    return {
        state : state,
        initializeRateCards: initializeRateCards,
        recalculateValuesAndRender: recalculateValuesAndRender,
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice
    }
})