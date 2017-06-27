﻿module.exports('rate-cards', function (require) {

    var totalObligation = require('financial-functions').totalObligation;
    var totalPrice = require('financial-functions').totalPrice;
    var totalAmountFinanced = require('financial-functions').totalAmountFinanced;
    var monthlyPayment = require('financial-functions').monthlyPayment;
    var totalMonthlyPayments = require('financial-functions').totalMonthlyPayments;
    var residualBalance = require('financial-functions').residualBalance;
    var totalBorrowingCost = require('financial-functions').totalBorrowingCost;
    var yourCost = require('financial-functions').yourCost;
    var tax = require('financial-functions').tax;
    var rateCardBlock = require('rate-cards-ui');

    var constants = require('state').constants;
    var state = require('state').state;

    var notNaN = function (num) { return !isNaN(num); };

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    var submitRateCard = function (option) {
        if (option === 'Deferral') {
            $('#LoanDeferralType').val($('#DeferralPeriodDropdown').val());
        } else {
            $('#LoanDeferralType').val('0');
        }

        //remove percentage symbol
        var amortizationTerm = $('#' + option + 'AmortizationDropdown').val();
        var loanTerm = amortizationTerm;

        var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
        var slicedAdminFee = $('#' + option + 'AFee').text().substring(1);
        var slicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);
        var cards = sessionStorage.getItem(state.contractId + option);
        if (cards !== null) {
            var cardType = $.grep(constants.rateCards, function(c) { return c.name === option; })[0].id;

            var filtred = $.grep($.parseJSON(cards),
                function(v) {
                    return v.CardType === cardType &&
                        v.AmortizationTerm === Number(amortizationTerm) &&
                        v.AdminFee === Number(slicedAdminFee) &&
                        v.CustomerRate === Number(slicedCustomerRate);
                })[0];

            if (filtred !== undefined) {
                $('#SelectedRateCardId').val(filtred.Id);
            }
        }

        $('#AmortizationTerm').val(amortizationTerm);
        $('#LoanTerm').val(loanTerm);
        $('#total-monthly-payment').val(slicedTotalMPayment);
        $('#CustomerRate').val(slicedCustomerRate);
        $('#AdminFee').val(slicedAdminFee);
    };

    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var monthlySum = function (equipments) {
        return Object.keys(equipments)
        .map(idToValue(equipments))
        .map(function (equipment) { return parseFloat(equipment.monthlyCost); })
        .filter(notNaN)
        .reduce(function (sum, cost) { return sum + cost;},0);
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

    var togglePromoLabel = function(option) {
        var isPromo = state[option.name].IsPromo;
        if (isPromo) {
            if ($('#' + option.name + 'Promo').is('.hidden')) {
                $('#' + option.name + 'Promo').removeClass('hidden');
            }
        } else {
            if (!$('#' + option.name + 'Promo').is('.hidden')) {
                $('#' + option.name + 'Promo').addClass('hidden');
            }
        }
    }

    var setBasicValues = function () {
        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        constants.rateCards.forEach(function (option) {
            var items = $.parseJSON(sessionStorage.getItem(state.contractId + option.name));

            if ((state.selectedCardId !== null && state.selectedCardId !== '0') && !state.isInitialized) {
                var selectedCard = $.grep(items,
                    function(card) {
                        return card.Id === Number(state.selectedCardId);
                    })[0];

                if (selectedCard !== null && selectedCard !== undefined) {
                    state[option.name] = selectedCard;
                    state[option.name].yourCost = '';

                    togglePromoLabel(option);

                    //just find parent div
                    toggleSelectedRateCard('#' + option.name + 'AFee'); 

                    $('#' + option.name + 'AmortizationDropdown').val(state[option.name].AmortizationTerm);
                    $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
                    $('#' + option.name + 'CRate').text(state[option.name].CustomerRate + ' %');
                    $('#' + option.name + 'YCostVal').text(state[option.name].DealerCost + ' %');
                    state.isInitialized = true;
                } else {
                    calculateRateCardValues(option, items);
                }
            } else {
                if (option.name === 'Custom' && !state.isInitialized && !state.isNewContract) {
                    $('#CustomLoanTerm').val($('#LoanTerm').val());
                    state[option.name].LoanTerm = Number($('#LoanTerm').val());
                    $('#CustomAmortTerm').val($('#AmortizationTerm').val());
                    state[option.name].AmortizationTerm = Number($('#AmortizationTerm').val());
                    var deferralPeriod = $.grep(constants.customDeferralPeriods, function (period) { return period.name === $('#LoanDeferralType').val() })[0];
                    $('#CustomDeferralPeriod').val(deferralPeriod === undefined ? 0 : deferralPeriod.val);
                    state[option.name].DeferralPeriod = deferralPeriod === undefined ? 0 : deferralPeriod.val;
                    $('#CustomCRate').val($('#CustomerRate').val());
                    state[option.name].CustomerRate = Number($('#CustomerRate').val());
                    $('#CustomAFee').val($('#AdminFee').val());
                    state[option.name].AdminFee = Number($('#AdminFee').val());
                    $('#CustomYCostVal').val($('#DealerCost').val());
                    state[option.name].DealerCost = Number($('#DealerCost').val());
                    toggleSelectedRateCard('#CustomLoanTerm');

                    state.isInitialized = true;
                } else {
                    calculateRateCardValues(option, items);
                }
            }
        });
        //constants.selectedCardId = null;
    }

    var renderOption = function (option, data, isRenderDropdowns) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = constants.numberFields.every(function (field) {
            var result = typeof data[field] === 'number';
            return result;
        });

        var validateNotEmpty = constants.notCero.every(function (field) {
            return data[field] !== 0;
        });

        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
            ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
            : '';
        
        if (selectedRateCard !== '') {
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            }
        }

        if (onlyCustomCard) {
            selectedRateCard = 'Custom';
        }

        if (notNan && validateNumber && validateNotEmpty) {
            renderDropdownValues(option, data.totalAmountFinanced, isRenderDropdowns);

            
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text(data["LoanTerm"]+ '/' + data["AmortizationTerm"]);
                $('#displayCustomerRate').text(data["CustomerRate"]);
                $('#displayTFinanced').text(formatCurrency(data.totalAmountFinanced));
                $('#displayMPayment').text(formatCurrency(data.monthlyPayment));
            }

            $('#' + option + 'MPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + 'CBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + 'TAFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + 'TMPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + 'RBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + 'TObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + 'YCost').text(formatCurrency(data.yourCost));
        } else {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text('-');
                $('#displayCustomerRate').text('-');
                $('#displayTFinanced').text('-');
                $('#displayMPayment').text('-');
            }
            $('#' + option + 'MPayment').text('-');
            $('#' + option + 'CBorrowing').text('-');
            $('#' + option + 'TAFinanced').text('-');
            $('#' + option + 'TMPayments').text('-');
            $('#' + option + 'RBalance').text('-');
            $('#' + option + 'TObligation').text('-');
            $('#' + option + 'YCost').text('-');
        }
    };

    var recalculateValuesAndRender = function (options, isRenderDropdowns) {
        var optionsToCompute = constants.rateCards;
        var renderDropdowns = isRenderDropdowns === undefined ? true : isRenderDropdowns;

        if (options !== undefined && options.length > 0) {
            optionsToCompute = options;
        }
        
        var eSum = equipmentSum(state.equipments);

        //setBasicValues();
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
            }), renderDropdowns);

        });
        setBasicValues();
    };

    var recalculateAndRenderRentalValues = function () {
        var eSum = monthlySum(state.equipments);
        
        var data = {
            tax: state.tax,
            equipmentSum: eSum
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#total-monthly-payment').val(formatNumber(eSum));
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalPrice(data)));
        } else {
            $('#total-monthly-payment').val('');
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };

    var recalculateRentalTaxAndPrice = function () {
        var data = {
            tax: state.tax,
            equipmentSum: state.rentalMPayment
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

    function renderDropdownValues(option, totalAmountFinanced, isRenderDropdowns) {
        var totalCash = constants.minimumLoanValue;
        if (totalAmountFinanced > 1000) {
            totalCash = totalAmountFinanced.toFixed(2);
        }

        if (isRenderDropdowns) {
            var items = $.parseJSON(sessionStorage.getItem(state.contractId + option));
            var dropdownValues;
            if (option === 'Deferral') {
                var dealerCost = state[option].DealerCost;
                dropdownValues = $.grep(items, function (card) {
                    return card.LoanValueFrom <= totalCash && card.LoanValueTo >= totalCash && card.DealerCost === dealerCost;
                });
            } else {
                dropdownValues = $.grep(items, function (card) {
                    return card.LoanValueFrom <= totalCash && card.LoanValueTo >= totalCash;
                });
            }

            var options = $('#' + option + 'AmortizationDropdown');
            options.empty();

            $.each(dropdownValues, function (item) {
                var optionTemplate = $("<option />").val(dropdownValues[item].AmortizationTerm).text(dropdownValues[item].LoanTerm + '/' + dropdownValues[item].AmortizationTerm);

                if (state[option].AmortizationTerm === dropdownValues[item].AmortizationTerm) {
                    optionTemplate.attr('selected', 'selected');
                }

                options.append(optionTemplate);
            });
        }
    }

    function calculateRateCardValues(option, items) {
        //minimum loan value
        var totalCash = constants.minimumLoanValue;
        var totalAmountFinanced = +$('#' + option.name + 'TAFinanced').text().substring(1);

        if (!isNaN(totalAmountFinanced)) {
            if (totalAmountFinanced > totalCash) {
                totalCash = totalAmountFinanced;
            }
        }
        
        var selectedValues = $('#' + option.name + 'AmortizationDropdown option:selected').text().split('/');
        var loanTerm = +(selectedValues[0]);
        var amortTerm = +(selectedValues[1]);
        var rateCard;
        if (option.name === 'Deferral') {
            var deferralPeriod = +$('#DeferralPeriodDropdown').val();
            rateCard = $.grep(items, function (i) {
                return i.DeferralPeriod === deferralPeriod && i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= totalCash;
            })[0];
        } else {
            rateCard = $.grep(items, function (i) {
                return i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= totalCash;
            })[0];
        }

        if (rateCard !== null && rateCard !== undefined) {

            state[option.name] = rateCard;
            state[option.name].yourCost = '';

            togglePromoLabel(option);
            if (option.name === 'Deferral') {
                $('#DeferralPeriodDropdown').val(state[option.name].DeferralPeriod);
            }

            $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
            $('#' + option.name + 'CRate').text(state[option.name].CustomerRate + ' %');
            $('#' + option.name + 'YCostVal').text(state[option.name].DealerCost + ' %');
        }

    }

    var initializeRateCards = function (id, cards) {
        state.contractId = id;
        state.isNewContract = $('#IsNewContract').val().toLowerCase() === 'true';

        renderRateCardUi(state.isNewContract);

        constants.rateCards.forEach(function (option) {
            if (sessionStorage.getItem(state.contractId + option.name) === null) {
                var filtred = $.grep(cards,
                    function (card) {
                        return card.CardType === option.id;
                    });
                sessionStorage.setItem(state.contractId + option.name, JSON.stringify(filtred));
            }

            if (option.name === 'Deferral' && (state.selectedCardId !== null && state.selectedCardId !== '0')) {
                var deferralPeriod = $.grep(constants.customDeferralPeriods, function (period) { return period.name === $('#LoanDeferralType').val() })[0];

                if (deferralPeriod != null && deferralPeriod.val !== 0) {
                    $('#DeferralPeriodDropdown').val(deferralPeriod.val.toString());
                }
            }

            setHandlers(option);
        });

        recalculateValuesAndRender([], true);
    }

    function toggleSelectedRateCard(selector) {
        if (!onlyCustomCard) {
            $(selector).parents('.rate-card').addClass('checked').siblings().removeClass('checked');
        }
    }

    function setHandlers(option) {
        $('#' + option.name + 'AmortizationDropdown').change(function () {
            $(this).prop('selected', true);

            recalculateValuesAndRender([], false);
        });
    }

    function renderRateCardUi(isNewContract) {
        rateCardBlock.toggle(isNewContract);

        if ($('#SelectedRateCardId').val() === "") {
            state.selectedCardId = null;
        } else {
            state.selectedCardId = $('#SelectedRateCardId').val();
        }

        if (state.isNewContract && state.selectedCardId === null) {
            $('#submit').addClass('disabled');
            $('#submit').parent().popover();
        } else {
            $('#submit').parent().popover('destroy');
        }
    }

    return {
        initializeRateCards: initializeRateCards,
        recalculateValuesAndRender: recalculateValuesAndRender,
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice,
        submitRateCard: submitRateCard
    }
})