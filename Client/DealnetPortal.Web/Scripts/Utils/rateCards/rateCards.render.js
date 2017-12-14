module.exports('rateCards.render', function (require) {
    var settings = {
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm']
    };

    var state = require('rateCards.state').state;
    var constants = require('rateCards.state').constants;

    var init = function(viewSettings) {
        $.extend(settings, viewSettings);
    }

    var renderOption = function (option, selectedRateCard, data) {
        if (_isEmpty(settings))
            throw new Error('settings are empty. Use init method first.');

        var notNan = !Object.keys(data).map(_idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = settings.numberFields.every(function (field) { return typeof data[field] === 'number'; });
        var validateNotEmpty = settings.notCero.every(function (field) { return data[field] !== 0; });

        if (notNan && validateNumber && validateNotEmpty) {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text(data.loanTerm + '/' + data.amortTerm);
                Object.keys(settings.displaySectionFields).map(function (key) { $('#' + key).text(formatCurrency(data[settings.displaySectionFields[key]])); });
            }

            Object.keys(settings.rateCardFields).map(function (key) { $('#' + option + key).text(formatCurrency(data[settings.rateCardFields[key]])); });
        } else {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text('-');
                Object.keys(settings.displaySectionFields).map(function (key) { $('#' + key).text('-'); });
            }
            Object.keys(settings.rateCardFields).map(function (key) { $('#' + option + key).text('-'); });
        }
    }

    var renderTotalPrice = function (data) {
        var notNan = !Object.keys(data).map(_idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan) {
            Object.keys(settings.totalPriceFields).map(function (key) { $('#' + key).text(formatNumber(data[settings.totalPriceFields[key]])); });
        } else {
            Object.keys(settings.totalPriceFields).map(function (key) { $('#' + key).text('-'); });
        }
    };

    var renderDropdownValues = function(option) {
        var totalCash = constants.minimumLoanValue;
        
        if (state[option].totalAmountFinanced > 1000) {
            totalCash = state[option].totalAmountFinanced.toFixed(2);
        }

        if (totalCash >= constants.maxRateCardLoanValue) {
            totalCash = constants.maxRateCardLoanValue;
        }

        var items = state.rateCards[option];

        if (!items)
            return;

        var dropdown = $('#' + option + '-amortDropdown')[0];
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
        var e = document.getElementById(option + '-amortDropdown');
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
            $.grep(dropdown.options, function (i) {
                values.push($(i).attr('value'));
            });

            if (values.indexOf(selected) !== -1) {
                e.value = selected;
                //$(dropdown).val(selected);
                $('#' + option + '-amortDropdown option[value=' + selected + ']').attr("selected", selected);
            } else {
                e.value = values[0];
                //$(dropdown).val(values[0]);
                $('#' + option + '-amortDropdown option[value=' + values[0] + ']').attr("selected", selected);
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

    var renderAfterFiltration = function(option, data) {
        if (option === 'Deferral') {
            $('#DeferralPeriodDropdown').val(data.deferralPeriod);
        }

        $('#' + option + 'AFee').text(formatCurrency(data.adminFee));
        $('#' + option + 'CRate').text(formatNumber(data.customerRate) + ' %');
        $('#' + option + 'YCostVal').text(formatNumber(data.dealerCost) + ' %');
    }

    function _isEmpty(obj) {
        if (obj === null) return true;

        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                return false;
            }
        }

        return Object.key(obj).length === 0;
    }

    function _idToValue(obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    return {
        init: init,
        renderAfterFiltration: renderAfterFiltration,
        renderDropdownValues: renderDropdownValues,
        renderOption: renderOption,
        renderTotalPrice: renderTotalPrice
    };
});