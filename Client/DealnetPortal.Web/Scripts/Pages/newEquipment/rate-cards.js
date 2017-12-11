'use strict';

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
    var constants = require('state').constants;
    var state = require('state').state;
	var rateCardBlock = require('rate-cards-ui');
	var totalRentalPrice = require('financial-functions').totalRentalPrice;
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
		var selectedValuescustom = $('#' + option + 'AmortizationDropdown option:selected').text().split('/');
		var loanTerm = selectedValuescustom[0];
		var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
		var slicedAdminFee = $('#' + option + 'AFee').text().replace('$', '').trim();
		var slicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);
        var cards = sessionStorage.getItem(state.contractId + option);
        if (cards !== null) {
            var cardType = $.grep(constants.rateCards, function(c) { return c.name === option; })[0].id;
            var filtred;

            if (option === 'Deferral') {
                var deferralPeriod = +$('#DeferralPeriodDropdown').val();
                filtred = $.parseJSON(cards).filter(
                    function (v) {
                        return v.CardType === cardType &&
                            v.DeferralPeriod === deferralPeriod &&
                            v.AmortizationTerm === Number(amortizationTerm) &&
                            v.AdminFee === (slicedAdminFee.indexOf(',') > -1 ? Globalize.parseNumber(slicedAdminFee) : Number(slicedAdminFee)) &&
                            v.CustomerRate === (slicedCustomerRate.indexOf(',') > -1 ? Globalize.parseNumber(slicedCustomerRate) : Number(slicedCustomerRate));
                    })[0];
            } else {
                filtred = $.parseJSON(cards).filter(
                    function (v) {
                        return v.CardType === cardType &&
                            v.AmortizationTerm === Number(amortizationTerm) &&
                            v.AdminFee === (slicedAdminFee.indexOf(',') > -1 ? Globalize.parseNumber(slicedAdminFee) : Number(slicedAdminFee)) &&
                            v.CustomerRate === (slicedCustomerRate.indexOf(',') > -1 ? Globalize.parseNumber(slicedCustomerRate) : Number(slicedCustomerRate));
                    })[0];
            }

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

    /**
     * Update all text field and global state object with new calculated values
     * @param {string} option  name of the cards [FixedRate,Deferral,NoInterst, Custom]
     * @param {Object<string>} data - new values
     * @returns {void} 
     */
    var renderOption = function (option, data) {
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

        if (state.onlyCustomRateCard) {
            selectedRateCard = 'Custom';
        }

        if (notNan && validateNumber && validateNotEmpty) {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text(state[option].LoanTerm + '/' + state[option].AmortizationTerm);
                $('#displayCustomerRate').text(formatNumber(state[option].CustomerRate));
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

    /**
     * recalculate all financial values for Loan agreement type
     * @param {Array<string>} options - object of options for recalucaltion, if empty recalculate all values on form 
     *  possible values [FixedRate,Deferral,NoInterst, Custom]
     * @returns {void} 
     */
    var recalculateValuesAndRender = function (options) {
        var optionsToCompute = constants.rateCards;

        if (options !== undefined && options.length > 0) {
            optionsToCompute = options;
        }
        
        var eSum = equipmentSum(state.equipments);

        renderTotalPrice({
            equipmentSum: eSum !== 0 ? eSum : '-',
            tax: eSum !== 0 ? tax({ equipmentSum: eSum, tax: state.tax }) : '-',
            totalPrice: eSum !== 0 ? totalPrice({ equipmentSum: eSum, tax: state.tax }) : '-'
        });

        var downPayment = $('#downPayment').val();
        state.downPayment = downPayment ? +downPayment : 0;

        optionsToCompute.forEach(function (option) {
            var data = $.extend({}, idToValue(state)(option.name),
                {
                    equipmentSum: eSum,
                    downPayment: state.downPayment,
                    tax: state.tax
                });

            calculateRateCardValues(option, $.extend({}, data, { totalAmountFinanced: totalAmountFinanced(data) }));

            if (option.name !== 'Custom') {
                renderDropdownValues(option.name, totalAmountFinanced(data));
            }

            data = $.extend({}, idToValue(state)(option.name),
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

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan && data.equipmentSum !== 0) {
            $('#total-monthly-payment').val(formatNumber(eSum));
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalRentalPrice(data)));
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
            $('#rentalTax').text(formatNumber(tax(data)));
            $('#rentalTMPayment').text(formatNumber(totalRentalPrice(data)));
        } else {
            $('#rentalTax').text('-');
            $('#rentalTMPayment').text('-');
        }
    };

    /**
     * Show/hide notification and disable dropdown option depending on totalAmountFinanced option
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {number} totalAmountFinanced - total cash value
     * @returns {void} 
     */
    function renderDropdownValues(option, totalAmountFinanced) {
        var totalCash = constants.minimumLoanValue;

        if (totalAmountFinanced > 1000) {
            totalCash = totalAmountFinanced.toFixed(2);
        }

        if (totalCash >= constants.maxRateCardLoanValue) {
            totalCash = constants.maxRateCardLoanValue;
        }

        var items = $.parseJSON(sessionStorage.getItem(state.contractId + option));

        if (!items)
            return;

        var dropdown = $('#' + option + 'AmortizationDropdown')[0];
        if (!dropdown || !dropdown.options) return;

        var dropdowns = [];
        var deferralValue = +$('#DeferralPeriodDropdown').val();

        $.each(items, function () {
            if (this.LoanValueTo >= totalCash && this.LoanValueFrom <= totalCash) {
                var dropdownValue = ~~this.LoanTerm + ' / ' + ~~this.AmortizationTerm;
                if (dropdowns.indexOf(dropdownValue) === -1) {
                    if (option === 'Deferral') {
                        if (~~this.DeferralPeriod === deferralValue) {
                            dropdowns.push(dropdownValue);
                        }
                    } else {
                        dropdowns.push(dropdownValue);
                    }
                }
            }
        });

        //var selected = $('#' + option + 'AmortizationDropdown option:selected').val();
        var e = document.getElementById(option + 'AmortizationDropdown');
        if (e !== undefined) {
            var selected = e.options[e.selectedIndex].value;

            $(dropdown).empty();
            $(dropdowns).each(function () {
                $("<option />", {
                    val: this.split('/')[1].trim(),
                    text: this
                }).appendTo(dropdown);
            });

            var values = [];
            $.grep(dropdown.options, function(i) {
                values.push($(i).attr('value'));
            });

            if (values.indexOf(selected) !== -1) {
                e.value = selected;
                //$(dropdown).val(selected);
                $('#' + option + 'AmortizationDropdown option[value=' + selected + ']').attr("selected", selected);
            } else {
                e.value = values[0];
                //$(dropdown).val(values[0]);
                $('#' + option + 'AmortizationDropdown option[value=' + values[0] + ']').attr("selected", selected);
            }
        }

        var options = dropdown.options;
        var tooltip = $('a#' + option + 'Notify');

        if (+totalCash >= constants.totalAmountFinancedFor180amortTerm) {
            toggleDisabledAttributeOnOption(option, options, tooltip, false);
        } else {
            toggleDisabledAttributeOnOption(option, options, tooltip, true);
        }
    }

    /**
     * depends on totalAmountfinanced value disable/enable options 
     * values of Loan/Amortization dropdown
     * @param {string} option - option name
     * @param {Array<string>} options - array of available options for dropdown
     * @param {Object<string>} tooltip - tooltip jqeury object for show/hide depends on totalAmountFinancedValue
     * @param {boolean} isDisable - disable/enable option and show/hide tooltip
     * @returns {void} 
     */
    function toggleDisabledAttributeOnOption(option, options, tooltip, isDisable) {
        if (!options.length) return;

        var formGroup = tooltip.closest('.form-group');
        if (isDisable) {
            formGroup.addClass('notify-hold');
            tooltip.show();
        } else {
            formGroup.removeClass('notify-hold');
            tooltip.hide();

            //check if notifications is visible if yes emulate click to hide it.
            //if (tooltip.parent().find('.popover-content').length) {
            //    tooltip.click();
            //}
        }
    }

    /**
     * Update current card for each of rate card options
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {Object<string>} data - calculated financial data
     * @returns {void} 
     */
    function calculateRateCardValues(option, data) {
        //minimum loan value
        var totalCash = constants.minimumLoanValue;
        var totalAmountFinanced = data.totalAmountFinanced;

        if (!isNaN(totalAmountFinanced)) {
            if (totalAmountFinanced > totalCash) {
                totalCash = totalAmountFinanced.toFixed(2);
            }
        }

        var rateCard = filterRateCardByValues(option, totalCash);

        if (rateCard !== null && rateCard !== undefined) {

            state[option.name] = rateCard;
            state[option.name].yourCost = '';

            rateCardBlock.togglePromoLabel(option.name);
            if (option.name === 'Deferral') {
                $('#DeferralPeriodDropdown').val(state[option.name].DeferralPeriod);
            }

            $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
            $('#' + option.name + 'CRate').text(formatNumber(state[option.name].CustomerRate) + ' %');
            $('#' + option.name + 'YCostVal').text(formatNumber(state[option.name].DealerCost) + ' %');
        }
    }

    /**
     * Select reate cards by current values of dropdown and totalCash
     * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
     * @param {number} totalCash - totalAmountFinancedValue for current option
     * @returns {Object<string>} - appropriate rate card object 
     */
    function filterRateCardByValues(option, totalCash) {
        var selectedValues = $('#' + option.name + 'AmortizationDropdown option:selected').text().split('/');
        var items = $.parseJSON(sessionStorage.getItem(state.contractId + option.name));
        if (!items)
            return null;

        var loanTerm = +selectedValues[0];
        var amortTerm = +selectedValues[1];
        if (option.name === 'Deferral') {
            var deferralPeriod = +$('#DeferralPeriodDropdown').val();
            return $.grep(items, function (i) {
                if (totalCash >= constants.maxRateCardLoanValue) {
                    return i.DeferralPeriod === deferralPeriod && i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= constants.maxRateCardLoanValue;
                } else {
                    return i.DeferralPeriod === deferralPeriod && i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= totalCash;
                }
            })[0];
        } else {
            return $.grep(items, function (i) {
                if (totalCash >= constants.maxRateCardLoanValue) {
                    return i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= constants.maxRateCardLoanValue;
                } else {
                    return i.AmortizationTerm === amortTerm && i.LoanTerm === loanTerm && i.LoanValueFrom <= totalCash && i.LoanValueTo >= totalCash;
                }
            })[0];
        }
    }

    return {
        recalculateValuesAndRender: recalculateValuesAndRender,
        recalculateAndRenderRentalValues: recalculateAndRenderRentalValues,
        recalculateRentalTaxAndPrice: recalculateRentalTaxAndPrice,
        submitRateCard: submitRateCard
    };
});