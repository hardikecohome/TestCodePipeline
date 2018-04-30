module.exports('rateCards.render', function (require) {
    var settings = {};

    var state = require('rateCards.state').state;
    var constants = require('rateCards.state').constants;

    var idToValue = require('idToValue');

    var init = function (viewSettings) {
        $.extend(settings, viewSettings);
    }

    var optionTemplates = {
        reductionDropdown: function(totalAmountFinanced) {
            return function() {
                var text;
                var value = (totalAmountFinanced * (this.InterestRateReduction / 100)).toFixed(2);
                if (this.Id === 0) {
                    text = translations.noReduction;
                } else
                {
                    text = '-' + this.CustomerReduction + '% ' + translations.for + ' ' + value + '$';
                }
                return {
                    val: this.Id,
                    text: text,
                    intRate: this.InterestRateReduction,
                    custRate: this.CustomerReduction
                }
            }
        },
        cardProgramDropdown: function() {
            return {
                val: this,
                text: this
            }
        },
        cardLoanAmortDropdown: function() {
            return {
                val: this.split('/')[1].trim(),
                text: this
            }
        }
    }

    var renderOption = function (option, selectedRateCard, data) {
        if (_isEmpty(settings))
            throw new Error('settings are empty. Use init method first.');

        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        var validateNumber = settings.numberFields.every(function (field) {
            return typeof data[field] === 'number';
        });
        var validateNotEmpty = settings.notCero.every(function (field) {
            return data[field] !== 0;
        });

        if (notNan && validateNumber && validateNotEmpty) {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text(data.loanTerm + '/' + data.amortTerm);
                $('#displayCustomerRate').text(data.CustomerRate.toFixed(2));
                Object.keys(settings.displaySectionFields).map(function (key) {
                    $('#' + key).text(formatCurrency(data[settings.displaySectionFields[key]]));
                });
            }

            Object.keys(settings.displaySectionFields).map(function (key) {
                $('#' + option + key).text(formatNumber(data[settings.displaySectionFields[key]]) + '%');
            });
            Object.keys(settings.rateCardFields).map(function (key) {
                $('#' + option + key).text(formatCurrency(data[settings.rateCardFields[key]]));
            });
        } else {
            if (option === selectedRateCard) {
                $('#displayLoanAmortTerm').text('-');
                $('#displayCustomerRate').text('-');
                Object.keys(settings.displaySectionFields).map(function (key) {
                    $('#' + key).text('-');
                });
            }
            Object.keys(settings.rateCardFields).map(function (key) {
                $('#' + option + key).text('-');
            });
        }
    }

    var renderTotalPrice = function (option, data) {
        var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) {
            return isNaN(val);
        });
        if (notNan) {
            Object.keys(settings.totalPriceFields).map(function (key) {
                $('#' + option + key).text(formatNumber(data[settings.totalPriceFields[key]]));
            });
        } else {
            Object.keys(settings.totalPriceFields).map(function (key) {
                $('#' + option + key).text('-');
            });
        }
    };

    var renderDropdownValues = function (dataObject) {
        var resultObj = _getItemsByPrice(dataObject);
        var totalCash = resultObj.totalCash;
        var items = resultObj.items;

        if (!items)
            return;

        var isStandalone = dataObject.hasOwnProperty('standaloneOption');

        var selectorName = isStandalone ? dataObject.standaloneOption : dataObject.rateCardPlan;
        var dropdownSelector = selectorName + '-amortDropdown';
        var dropdown = $('#' + dropdownSelector)[0];

        if (!dropdown || !dropdown.options) return;

        var dropdowns = [];
        var deferralValue;

        if ($('#DeferralPeriodDropdown').length) {
            deferralValue = +$('#DeferralPeriodDropdown').val();
        } else {
            deferralValue = +$('#' + selectorName + '-deferralDropdown').val();
        }
        if (isStandalone) {
            var program = $('#' + selectorName + '-programDropdown').val();
            items = items.filter(function(item) {
                if (item.CustomerRiskGroup == null || program === '') {
                    return item.CustomerRiskGroup == null;
                } else {
                    return item.CustomerRiskGroup.GroupName === program;
                }
            });
        }

        $.each(items, function () {
            if (this.LoanValueTo >= totalCash && this.LoanValueFrom <= totalCash) {
                var dropdownValue = ~~this.LoanTerm + ' / ' + ~~this.AmortizationTerm;
                if (dropdowns.indexOf(dropdownValue) === -1) {
                    if (dataObject.rateCardPlan === 'Deferral') {
                        if (~~this.DeferralPeriod === deferralValue) {
                            dropdowns.push(dropdownValue);
                        }
                    } else {
                        dropdowns.push(dropdownValue);
                    }
                }
            }
        });

        _buildDropdownValues(dropdownSelector, dropdown, dropdowns, optionTemplates.cardLoanAmortDropdown);

        //var options = dropdown.options;
        //var tooltip = $('a#' + selectorName + 'Notify');

        //if (+totalCash >= constants.totalAmountFinancedFor180amortTerm) {
        //    toggleDisabledAttributeOnOption(options, tooltip, false);
        //} else {
        //    toggleDisabledAttributeOnOption(options, tooltip, true);
        //}
    }

    var renderProgramDropdownValues = function(dataObject) {
        var resultObj = _getItemsByPrice(dataObject);
        var totalCash = resultObj.totalCash;
        var items = resultObj.items;

        var selectorName = dataObject.hasOwnProperty('standaloneOption') ? dataObject.standaloneOption : dataObject.rateCardPlan;
        var dropdownSelector = selectorName + '-programDropdown';
        var dropdown = _getDropdown(dropdownSelector);

        if (!dropdown || !dropdown.options) return;

        var dropdowns = [];

        $.each(items, function () {
            if (this.LoanValueTo >= totalCash && this.LoanValueFrom <= totalCash) {
                if (this.CustomerRiskGroup != null) {
                    var dropdownValue = this.CustomerRiskGroup.GroupName;
                    if (dropdowns.indexOf(dropdownValue) === -1) {
                        dropdowns.push(dropdownValue);
                    }
                }
            }
        });

        if (!dropdowns.length) {
            if (!$('#' + dropdownSelector).is(':disabled')) {
                $(dropdown).empty();
                $('#' + dropdownSelector).attr('disabled', true);
            }
        } else {
            if ($('#' + dropdownSelector).is(':disabled')) {
                $('#' + dropdownSelector).attr('disabled', false);
            }

            _buildDropdownValues(dropdownSelector, dropdown, dropdowns.reverse(), optionTemplates.cardProgramDropdown);
        }
    }

    var renderReductionDropdownValues = function(dataObject) {
        var isStandalone = dataObject.hasOwnProperty('standaloneOption');

        var selectorName = isStandalone ? dataObject.standaloneOption : dataObject.rateCardPlan;
        var dropdownSelector = selectorName + '-reduction';
        var dropdown = _getDropdown(dropdownSelector);

        if (!dropdown || !dropdown.options) return;

        if (!isStandalone && dataObject.reductionId != null) {
            $(dropdown).val(dataObject.reductionId);
        }

        var loanAmortDropdpownValues = $('#' + selectorName + '-amortDropdown option:selected').text();
        var totalAmountFinanced = state[dataObject.rateCardPlan].totalAmountFinanced.toFixed(2);

        //remove spaces in text
        loanAmortDropdpownValues = loanAmortDropdpownValues.replace(/\s+/g,'');

        var reductionValues = state.rateCardReduction.filter(function(reduction) {
            return reduction.LoanAmortizationTerm === loanAmortDropdpownValues && reduction.CustomerReduction <= dataObject.customerRate;
        });

        reductionValues.unshift({ Id: 0, InterestRateReduction: 0, CustomerReduction: 0 });
        _buildDropdownValues(dropdownSelector, dropdown, reductionValues, optionTemplates.reductionDropdown(dataObject.totalAmountFinanced));
    }

    function _buildDropdownValues(selector, dropdown, items, template) {
        var e = document.getElementById(selector);
        if (e !== undefined) {
            var selected = e.selectedIndex !== -1 ? e.options[e.selectedIndex].value : '';

            $(dropdown).empty();
            $(items).each(function () {
                $("<option />", template.call(this)).appendTo(dropdown);
            });

            var values = [];
            $.grep(dropdown.options, function (i) {
                values.push($(i).attr('value'));
            });

            e.value = values.indexOf(selected) !== -1 ? selected : values[0];
            var $selectedOption = $('#' + selector + ' option[value="' + e.value + '"]');
            $selectedOption.attr("selected", selected);

            if ($('#' + selector).hasClass('not-selected')) {
                $('#' + selector).removeClass('not-selected');
            }
        }
    }

    function _getItemsByPrice(dataObject) {
        var totalCash = _getTotalAmounFinanced(dataObject);

        var items = state.rateCards[dataObject.rateCardPlan];

        return {
            items: items,
            totalCash: totalCash
        }
    }

    function _getDropdown(selector) {
        var dropdown = $('#' + selector)[0];

        return dropdown;
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
    function toggleDisabledAttributeOnOption(options, tooltip, isDisable) {
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

    function _getTotalAmounFinanced(dataObject) {
        var totalCash = constants.minimumLoanValue;

        var isStandalone = dataObject.hasOwnProperty('standaloneOption');

        var totalAmountFinanced = isStandalone ? dataObject.totalAmountFinanced : state[dataObject.rateCardPlan].totalAmountFinanced.toFixed(2);

        if (totalAmountFinanced > 1000) {
            totalCash = totalAmountFinanced;
        }

        if (totalCash >= constants.maxRateCardLoanValue) {
            totalCash = constants.maxRateCardLoanValue;
        }

        return totalCash;
    }

    var renderAfterFiltration = function (option, data) {
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

    return {
        init: init,
        renderAfterFiltration: renderAfterFiltration,
        renderDropdownValues: renderDropdownValues,
        renderProgramDropdownValues: renderProgramDropdownValues,
        renderReductionDropdownValues: renderReductionDropdownValues,
        renderOption: renderOption,
        renderTotalPrice: renderTotalPrice
    };
});