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
    var selectedCardId;
    var isInialized = false;
    var isNewContract = true;
    var rateCards = [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }, { id: 3, name: 'Custom' }];
    var requiredCustomRateCardField = ['CustomCRate', 'CustomAmortTerm', 'CustomLoanTerm', 'CustomYCostVal', 'CustomAFee'];
    var customDeferralPeriods = [{ val: 0, name: 'NoDeferral' }, { val: 3, name: 'ThreeMonth' }, { val: 6, name: 'SixMonth' }, { val: 9, name: 'NineMonth' }, { val: 12, name: 'TwelveMonth' }];
    var numberFields = ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'AdminFee'];
    var notCero = ['equipmentSum', 'LoanTerm', 'AmortizationTerm'];

    window.state = {
        agreementType: 0,
        equipments: { },
        tax: taxRate,
        downPayment: 0,
        rentalMPayment: 0,
        Custom: {
            LoanTerm: '',
            AmortizationTerm: '',
            DeferralPeriod: 0,
            CustomerRate: '',
            yourCost: '',
            AdminFee: 0
        }
    };

    var validateCustomRateCardOnSubmit = function () {
        var isValid = requiredCustomRateCardField.every(function (field) {
            return $("#" + field).valid();
        });

        return isValid;
    }

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
        rateCards.forEach(function (option) {
            var items = $.parseJSON(sessionStorage.getItem(contractId + option.name));

            if ((selectedCardId !== null && selectedCardId !== '0') && !isInialized) {
                var selectedCard = $.grep(items,
                    function(card) {
                        return card.Id === Number(selectedCardId);
                    })[0];

                if (selectedCard !== null && selectedCard !== undefined) {
                    state[option.name] = selectedCard;
                    state[option.name].yourCost = '';

                    togglePromoLabel(option);

                    //just find parent div
                    toggleSelectedRateCard('#' + option.name + 'AFee');
                    if (option.name === 'Deferral') {
                        $('#DeferralPeriodDropdown').val(state[option.name].DeferralPeriod);
                    }

                    $('#' + option.name + 'AmortizationDropdown').val(state[option.name].AmortizationTerm);
                    $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
                    $('#' + option.name + 'CRate').text(state[option.name].CustomerRate + ' %');
                    $('#' + option.name + 'YCostVal').text(state[option.name].DealerCost + ' %');
                    isInialized = true;
                } else {
                    calculateRateCardValues(option, items);
                }
            } else {
                if (option.name === 'Custom' && !isInialized && !isNewContract) {
                    $('#CustomLoanTerm').val($('#LoanTerm').val());
                    state[option.name].LoanTerm = Number($('#LoanTerm').val());
                    $('#CustomAmortTerm').val($('#AmortizationTerm').val());
                    state[option.name].AmortizationTerm = Number($('#AmortizationTerm').val());
                    var deferralPeriod = $.grep(customDeferralPeriods, function (period) { return period.name === $('#LoanDeferralType').val() })[0];
                    $('#CustomDeferralPeriod').val(deferralPeriod === undefined ? 0 : deferralPeriod.val);
                    state[option.name].DeferralPeriod = deferralPeriod === undefined ? 0 : deferralPeriod.val;
                    $('#CustomCRate').val($('#CustomerRate').val());
                    state[option.name].CustomerRate = Number($('#CustomerRate').val());
                    $('#CustomAFee').val($('#AdminFee').val());
                    state[option.name].AdminFee = Number($('#AdminFee').val());
                    $('#CustomYCost').val(0);
                    state[option.name].yourCost = 0;
                    toggleSelectedRateCard('#CustomLoanTerm');

                    isInialized = true;
                } else {
                    calculateRateCardValues(option, items);
                }
            }
        });
        selectedCardId = null;
    }

    var renderOption = function (option, data, isRenderDropdowns) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = numberFields.every(function (field) {
            var result = typeof data[field] === 'number';
            return result;
        });

        var validateNotEmpty = notCero.every(function (field) {
            return data[field] !== 0;
        });

        var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
            ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
            : '';
        if (selectedRateCard !== '') {
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
            }
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
        var optionsToCompute;
        var renderDropdowns = isRenderDropdowns === undefined ? true : isRenderDropdowns;

        if (options !== undefined && options.length > 0) {
            optionsToCompute = options;
        } else {
            optionsToCompute = rateCards;
        }
        var eSum = equipmentSum(state.equipments);

        setBasicValues();
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
    };

    var recalculateAndRenderRentalValues = function () {
        var eSum = equipmentSum(state.equipments);

        var data = {
            tax: state.tax,
            equipmentSum: eSum
        };

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#rentalMPayment').val(formatNumber(eSum));
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalPrice(data)));
        } else {
            $('#rentalMPayment').val('');
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
        var totalCash = 1000;
        if (totalAmountFinanced > 1000) {
            totalCash = totalAmountFinanced;
        }

        if (isRenderDropdowns) {
            var items = $.parseJSON(sessionStorage.getItem(contractId + option));
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
                options.append($("<option />").val(dropdownValues[item].AmortizationTerm).text(dropdownValues[item].LoanTerm + '/' + dropdownValues[item].AmortizationTerm));
            });
        }
    }

    function calculateRateCardValues(option, items) {
        //minimum loan value
        var totalCash = 1000;
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
        contractId = id;
        isNewContract = $('#IsNewContract').val().toLowerCase() === 'true';

        renderRateCardUi(isNewContract);

        rateCards.forEach(function (option) {
            if (sessionStorage.getItem(contractId + option.name) === null) {
                var filtred = $.grep(cards,
                    function (card) {
                        return card.CardType === option.id;
                    });
                sessionStorage.setItem(contractId + option.name, JSON.stringify(filtred));
            }

            setHandlers(option);
        });
        recalculateValuesAndRender([], true);
    }

    function toggleSelectedRateCard(selector) {
        $(selector).parents('.rate-card').addClass('checked').siblings().removeClass('checked');
    }

    function setHandlers(option) {
        $('#' + option.name + 'AmortizationDropdown').change(function () {
            $(this).prop('selected', true);

            recalculateValuesAndRender([], false);
        });
    }

    function renderRateCardUi(isNewContract) {
        setValidationOnCustomRateCard();

        if (isNewContract) {
            $('#rateCardsBlock').show('slow', function () {
                $('#loanRateCardToggle').find('i.glyphicon')
                    .removeClass('glyphicon-chevron-right')
                    .addClass('glyphicon-chevron-down');
            });


        } else {
            $('#rateCardsBlock').hide('slow', function () {
                $('#loanRateCardToggle').find('i.glyphicon')
                    .removeClass('glyphicon-chevron-down')
                    .addClass('glyphicon-chevron-right');
            });
        }

        if ($('#SelectedRateCardId').val() === "") {
            selectedCardId = null;
        } else {
            selectedCardId = $('#SelectedRateCardId').val();
        }

        if (isNewContract && selectedCardId === null) {
            $('#submit').addClass('disabled');
        }
    }

    function setValidationOnCustomRateCard() {
        requiredCustomRateCardField.forEach(function(input) {
            $('#' + input).rules('add',
                {
                    required: true,
                    minlength: 1,
                    regex: /^[0-9]\d{0,11}([.,][0-9][0-9]?)?$/
                });
        });
    }

    return {
        state: state,
        initializeRateCards: initializeRateCards,
        recalculateValuesAndRender: recalculateValuesAndRender,
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice,
        rateCards: rateCards,
        idToValue: idToValue,
        validateCustomRateCard : validateCustomRateCardOnSubmit
    }
})