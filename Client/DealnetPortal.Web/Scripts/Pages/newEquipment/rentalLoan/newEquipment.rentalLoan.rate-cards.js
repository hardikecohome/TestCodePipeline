'use strict';

module.exports('rate-cards', function (require) {
    var constants = require('state').constants;
    var state = require('state').state;
    var rateCardsCalculator = require('rateCards.index');
    var rateCardsRenderEngine = require('rateCards.render');
    var customRateCard = require('custom-rate-card');

    var settings = {
        rateCardFields: {
            'MPayment': 'monthlyPayment',
            'CBorrowing': 'costOfBorrowing',
            'TAFinanced': 'totalAmountFinanced',
            'TMPayments': 'totalMonthlyPayments',
            'RBalance': 'residualBalance',
            'TObligation': 'totalObligation',
            'YCost': 'yourCost',
            'AFee': 'adminFee'
        },
        totalPriceFields: {
            'totalEquipmentPrice': 'equipmentSum',
            'tax': 'tax',
            'totalPrice': 'totalPrice'
        },
        displaySectionFields: {
            //'displayCustomerRate': 'CustomerRate',
            'displayTFinanced': 'totalAmountFinanced',
            'displayMPayment': 'monthlyPayment'
        },
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm'],
        reductionCards: ['FixedRate', 'Deferral']
    }

    var idToValue = require('idToValue');

    var submitRateCard = function (option) {
        if (option === 'Deferral') {
            $('#LoanDeferralType').val($('#DeferralPeriodDropdown').val());
        } else {
            $('#LoanDeferralType').val('0');
        }

        //remove percentage symbol
        var amortizationTerm = $('#' + option + '-amortDropdown').val();
        var selectedValuescustom = $('#' + option + '-amortDropdown option:selected').text().split('/');
        var loanTerm = selectedValuescustom[0];
        var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
        var slicedAdminFee = $('#' + option + 'AFee').text().replace('$', '').trim();
        var slicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);

        var cardType = $.grep(constants.rateCards, function (c) {
            return c.name === option;
        })[0].name;
        $('#SelectedRateCardId').val(state[cardType].Id);

        if (settings.reductionCards.indexOf(cardType) !== -1) {
            $('#rateReductionId').val(state[option].ReductionId);
            $('#rateReduction').val(state[option].CustomerReduction);
            $('#rateReductionCost').val(state[option].CustomerReductionCost);
        }

        $('#AmortizationTerm').val(amortizationTerm);
        $('#LoanTerm').val(loanTerm);
        $('#total-monthly-payment').val(slicedTotalMPayment);
        $('#total-monthly-payment-display').text(slicedTotalMPayment);
        $('#CustomerRate').val(slicedCustomerRate);
        $('#AdminFee').val(slicedAdminFee);
    };

    /**
     * recalculate all financial values for Loan agreement type
     * @param {Array<string>} options - object of options for recalucaltion, if empty recalculate all values on form 
     *  possible values [FixedRate,Deferral,NoInterst, Custom]
     * @returns {void} 
     */
    var recalculateValuesAndRender = function (options) {
        var optionsToCompute = constants.rateCards;
        var maxCreditAmount = $("#max-credit-amount").val();
        var isCapOutMaxAmt = $("#isCapOutMaxAmt").val();
        if (options !== undefined && options.length > 0) {
            optionsToCompute = options;
        }

        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        var eSumData = rateCardsCalculator.calculateTotalPrice(state.equipments, state.downPayment, state.tax);

        rateCardsRenderEngine.renderTotalPrice('', eSumData);

        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0 ?
            $('#rateCardsBlock').find('div.checked').find('#hidden-option').text() :
            '';

        if (selectedRateCard !== '') {
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            }
        }

        optionsToCompute.forEach(function (option) {
            var rateCard = rateCardsCalculator.filterRateCard({
                rateCardPlan: option.name
            });

            if (rateCard !== null && rateCard !== undefined) {

                var obj = {CustomerReduction: 0, InterestRateReduction: 0, ReductionId: null };
                if (state[option.name]) {
                    obj = {
                        CustomerReduction: !state[option.name].CustomerReduction ? 0 : state[option.name].CustomerReduction,
                        InterestRateReduction: !state[option.name].InterestRateReduction ? 0 : state[option.name].InterestRateReduction
                    }
                }

                var reductionId = !state[option.name] || !state[option.name].ReductionId ? null : state[option.name].ReductionId;
                var customerReduction = !state[option.name] || !state[option.name].CustomerReduction ? null : state[option.name].CustomerReduction;
                var interestRateReduction = !state[option.name] || !state[option.name].InterestRateReduction ? null : state[option.name].InterestRateReduction;
                state[option.name] = $.extend(true, obj, rateCard);
                state[option.name].ReductionId = reductionId;
                state[option.name].CustomerReduction = customerReduction;
                state[option.name].InterestRateReduction = interestRateReduction;
                state[option.name].CustomerReduction = !state[option.name].CustomerReduction ? 0 : state[option.name].CustomerReduction,
                state[option.name].InterestRateReduction = !state[option.name].InterestRateReduction ? 0 : state[option.name].InterestRateReduction,
                state[option.name].yourCost = '';

                if (settings.reductionCards.indexOf(option.name) !== -1) {
                    var taf = rateCardsCalculator.getTotalAmountFinanced({
                        includeAdminFee: state.isCoveredByCustomer,
                        AdminFee: state[option.name].AdminFee
                    });

                    rateCardsRenderEngine.renderReductionDropdownValues({
                        rateCardPlan: option.name,
                        customerRate: state[option.name].CustomerRate,
                        reductionId: state[option.name].ReductionId,
                        totalAmountFinanced: taf
                    });

                    var reducedCustomerRate = state[option.name].CustomerRate - state[option.name].CustomerReduction;
                    state[option.name].CustomerRate = reducedCustomerRate;
                }

                rateCardsRenderEngine.renderAfterFiltration(option.name, {
                    deferralPeriod: state[option.name].DeferralPeriod,
                    adminFee: state[option.name].AdminFee,
                    dealerCost: state[option.name].DealerCost,
                    customerRate: state[option.name].CustomerRate
                });
            }
            if (option.name !== 'Custom') {
                rateCardsRenderEngine.renderDropdownValues({
                    rateCardPlan: option.name
                });
            }

            var includeAdminFeeInCalc = state.isCoveredByCustomer;
            if (option.name === 'Custom') {
                customRateCard.setAdminFeeByEquipmentSum(eSumData.totalPrice !== "-" ? eSumData.totalPrice : 0);
            }

            var data = rateCardsCalculator.calculateValuesForRender($.extend({
                includeAdminFee: includeAdminFeeInCalc
            }, idToValue(state)(option.name)));
            var totalAmountFinance = data.totalAmountFinanced;
            state[option.name].CustomerReductionCost = data.tafBuyDownRate;
            if (isCapOutMaxAmt == 'True' && totalAmountFinance > maxCreditAmount) {
                $('#max-amt-cap-out-error').show();
                $('#submit').addClass('disabled');
                $('#submit').parent().popover();
            } else {
                $('#max-amt-cap-out-error').hide();
            }
            rateCardsRenderEngine.renderOption(option.name, selectedRateCard, data);
        });
    };

    var init = function () {
        rateCardsRenderEngine.init(settings);
    }

    return {
        recalculateValuesAndRender: recalculateValuesAndRender,
        submitRateCard: submitRateCard,
        init: init
    };
});