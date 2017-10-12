module.exports('calculator-index', function (require) {
    var totalObligation = require('financial-functions').totalObligation;
    var totalPrice = require('financial-functions').totalPrice;
    var totalAmountFinanced = require('financial-functions').totalAmountFinanced;
    var monthlyPayment = require('financial-functions').monthlyPayment;
    var totalMonthlyPayments = require('financial-functions').totalMonthlyPayments;
    var residualBalance = require('financial-functions').residualBalance;
    var totalBorrowingCost = require('financial-functions').totalBorrowingCost;
    var yourCost = require('financial-functions').yourCost;
    var tax = require('financial-functions').tax;

    var setup = require('calculator-option').optionSetup;
    var render = require('calculator-option').renderOption;
    var add = require('calculator-option').addOption;
    var setTax = require('calculator-value-setters').setTax;
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var jcarousel = require('calculator-jcarousel');

    var notNaN = function (num) { return !isNaN(num); };

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    var renderTotalPrice = function (option, data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });

        $('#' + option + '-taxDescription').text(state.description);

        if (notNan) {

            $('#' + option + '-totalEquipmentPrice').text(formatNumber(data.equipmentSum));
            $('#' + option + '-tax').text(formatNumber(data.tax));
            $('#' + option + '-totalPrice').text(formatNumber(data.totalPrice));
        } else {
            $('#' + option + '-totalEquipmentPrice').text('-');
            $('#' + option + '-tax').text('-');
            $('#' + option + '-totalPrice').text('-');
        }
    };

    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var calculate = function (options) {
        if (options === undefined || typeof options !== "object") {
            options = ['option1'];
            if (state['option2'] !== undefined) {
                options.push('option2');
            }
            if (state['option3'] !== undefined) {
                options.push('option3');
            }
        }

        options.forEach(function (option) {
            var eSum = equipmentSum(state[option].equipments);

            var data = $.extend({}, idToValue(state)(option),
                {
                    equipmentSum: eSum,
                    downPayment: state.downPayment,
                    tax: state.tax
                });

            selectRateCard(option, totalAmountFinanced(data));
            $.extend(data, idToValue(state)(option));

            renderTotalPrice(option, {
                equipmentSum: eSum !== 0 ? eSum : '-',
                tax: eSum !== 0 ? tax({ equipmentSum: eSum, tax: state.tax }) : '-',
                totalPrice: eSum !== 0 ? totalPrice({ equipmentSum: eSum, tax: state.tax }) : '-'
            });

            render(option, $.extend({}, data, {
                monthlyPayment: monthlyPayment(data),
                costOfBorrowing: totalBorrowingCost(data),
                totalAmountFinanced: totalAmountFinanced(data),
                totalMonthlyPayments: totalMonthlyPayments(data),
                residualBalance: residualBalance(data),
                totalObligation: totalObligation(data),
                yourCost: yourCost(data),
                adminFee: state[option].AdminFee,
                customerRate: state[option].CustomerRate,
                dealerCost: state[option].DealerCost
            }));
        });
    }

    var initialSetup = function () {
        var name = 'option1';

        $('#province-tax-rate').on('change', setTax(calculate));

        selectRateCard(name);
        setup(name, calculate);

        $('.btn-add-calc-option').on('click', function () {
            add(calculate);
        });
    }

    $(window).resize(function () {
        jcarousel.carouselRateCards();
        jcarousel.refreshCarouselItems();
    });

    $(function () {
        jcarousel.carouselRateCards();
        initialSetup();
    });


    /**
     * Update current card for each of rate card options
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {Object<>} data - calculated financial data
     * @returns {} 
     */
    function selectRateCard(option, totalFinanced) {
        //minimum loan value
        var totalCash = 1000;
        var totalAmountFinanced = totalFinanced;

        if (!isNaN(totalAmountFinanced)) {
            if (totalAmountFinanced > totalCash) {
                totalCash = totalAmountFinanced.toFixed(2);
            }
        }
        
        var rateCard = filterRateCardByValues(option, totalCash);

        if (rateCard !== null && rateCard !== undefined) {

            $.extend(state[option], rateCard);
        }
    }

    /**
     * Select reate cards by current values of dropdown and totalCash
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {number} totalCash - totalAmountFinancedValue for current option
     * @returns {Object<>} - appropriate rate card object 
     */
    function filterRateCardByValues(option, totalCash) {
        var items = state.cards;

        if (!items)
            return null;

        if (state[option].plan === 3 && state[option].Id !== '')
            return constants.emptyRateCard;

        if (state[option].plan === 3)
            return null;

        var selectedValues = $('#' + option + '-amortDropdown option:selected').text().split('/');
        var selectedPlan = state[option].plan;

        var loanTerm = +(selectedValues[0]);
        var amortTerm = +(selectedValues[1]);

        var card;
        if (selectedPlan === 2) {
            var deferralPeriod = state[option].DeferralPeriod;
            card = $.grep(items,
                function(i) {
                    if (totalCash >= constants.maxRateCardLoanValue) {
                        return i.CardType === state[option].plan &&
                            i.AmortizationTerm === amortTerm &&
                            i.LoanTerm === loanTerm &&
                            i.LoanValueFrom <= totalCash &&
                            i.LoanValueTo >= constants.maxRateCardLoanValue &&
                            i.DeferralPeriod === deferralPeriod;
                    } else {
                        return i.CardType === state[option].plan &&
                            i.AmortizationTerm === amortTerm &&
                            i.LoanTerm === loanTerm &&
                            i.LoanValueFrom <= totalCash &&
                            i.LoanValueTo >= totalCash && 
                            i.DeferralPeriod === deferralPeriod;
                    }
                })[0];
        } else {
            card = $.grep(items,
                function(i) {
                    if (totalCash >= constants.maxRateCardLoanValue) {
                        return i.CardType === state[option].plan &&
                            i.AmortizationTerm === amortTerm &&
                            i.LoanTerm === loanTerm &&
                            i.LoanValueFrom <= totalCash &&
                            i.LoanValueTo >= constants.maxRateCardLoanValue;
                    } else {
                        return i.CardType === state[option].plan &&
                            i.AmortizationTerm === amortTerm &&
                            i.LoanTerm === loanTerm &&
                            i.LoanValueFrom <= totalCash &&
                            i.LoanValueTo >= totalCash;
                    }
                })[0];
        }
        return card;

    }

});