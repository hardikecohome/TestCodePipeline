module.exports('calculator-option', function (require) {
    var setters = require('calculator-value-setters');
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var ui = require('calculator-ui');
    var recalculateEquipmentIndex = require('calculator-conversion').recalculateEquipmentIndex;
    var recalculateEquipmentId = require('calculator-conversion').recalculateEquipmentId;
    var jcarousel = require('calculator-jcarousel');

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    function toggleClearInputIcon(fields) {
        var fields = fields || $('.control-group input, .control-group textarea');
        var fieldParent = fields.parent('.control-group:not(.date-group):not(.control-group-pass)');
        fields.each(function () {
            toggleClickInp($(this));
        });
        fields.on('keyup', function () {
            toggleClickInp($(this));
        });
        fieldParent.find('.clear-input').on('click', function () {
            $(this).siblings('input, textarea').val('').change();
            $(this).hide();
        });
    }

    var optionSetup = function (option, callback) {

        $('#' + option + '-addEquipment').on('click', ui.addEquipment(option, callback));

        $('#' + option + '-downPayment').on('change', setters.setDownPayment(option, callback));
        $('#' + option + '-plan').on('change', setters.setRateCardPlan(option, callback));
        $('#' + option + '-amortDropdown').on('change', setters.setLoanAmortTerm(option, callback));
        $('#' + option + '-deferralDropdown').on('change', setters.setDeferralPeriod(option, callback));
        $('#' + option + '-customLoanTerm').on('change', setters.setLoanTerm(option, callback));
        $('#' + option + '-customAmortTerm').on('change', setters.setAmortTerm(option, callback));
        $('#' + option + '-customCRate').on('change', setters.setCustomerRate(option, callback));
        $('#' + option + '-customYCostVal').on('change', setters.setYourCost(option, callback));
        $('#' + option + '-customAFee').on('change', setters.setAdminFee(option, callback));

        if (option === 'option1') {
            var nextIndex = state[option].equipmentNextIndex;

            state[option].equipments[nextIndex] = { id: nextIndex.toString(), cost: '', description: '' };
            state[option].equipments[nextIndex].type = $('#Equipment_NewEquipment_' + nextIndex + '__Type').val();

            var container = $('#option1-equipment-0');

            container.find('input, select, textarea').each(function () {
                $(this).attr('id', $(this).attr('id').replace('Equipment_NewEquipment_0', 'Equipment_NewEquipment_option1_0'));
                $(this).attr('name', $(this).attr('name').replace('Equipment.NewEquipment[0', 'Equipment.NewEquipment[option1_0'));
            });

            container.find('label').each(function () {
                $(this).attr('for', $(this).attr('for').replace('Equipment_NewEquipment_0', 'Equipment_NewEquipment_option1_0'));
            });

            container.find('span').each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }

                $(this).attr('data-valmsg-for', valFor.replace('Equipment.NewEquipment[0', 'Equipment.NewEquipment[option1_0'));
            });

            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Cost').on('change', setters.setEquipmentCost(option, callback));
            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Description').on('change', setters.setEquipmentDescription(option));
            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Type').on('change', setters.setEquipmentType(option));
            $('#option1-remove').on('click', function () {
                removeOption.call(this, callback);
            });

            state[option].equipmentNextIndex++;

            $('#' + option + '-plan').change();
        }

        $('#' + option + '-container').validate();

        initValidation(option);
    }

    var renderOption = function(option, data) {
        var validateNumber = constants.numberFields.every(function (field) {
            var result = typeof data[field] === 'number';
            return result;
        });
        var notNan = !Object.keys(data).map(idToValue(data)).some(function(val) {
            if (typeof val !== 'object') {
                return isNaN(val);
            }
        });

        var validateNotEmpty = constants.notCero.every(function (field) {
            return data[field] !== 0;
        });

        $('#' + option + '-aFee').text(data.adminFee);
        $('#' + option + '-cRate').text(data.customerRate + ' %');
        $('#' + option + '-yCostVal').text(data.dealerCost + ' %');

        if (state[option].plan === 0 || state[option].plan === 2) {
            renderDropdownValues(option, data.totalAmountFinanced);
        }

        if (notNan && validateNumber && validateNotEmpty) {
            $('#' + option + '-mPayment').text(formatCurrency(data.monthlyPayment));
            $('#' + option + '-cBorrowing').text(formatCurrency(data.costOfBorrowing));
            $('#' + option + '-taFinanced').text(formatCurrency(data.totalAmountFinanced));
            $('#' + option + '-tmPayments').text(formatCurrency(data.totalMonthlyPayments));
            $('#' + option + '-rBalance').text(formatCurrency(data.residualBalance));
            $('#' + option + '-tObligation').text(formatCurrency(data.totalObligation));
            $('#' + option + '-yCost').text(formatCurrency(data.yourCost));
        } else {
            $('#' + option + '-mPayment').text('-');
            $('#' + option + '-cBorrowing').text('-');
            $('#' + option + '-taFinanced').text('-');
            $('#' + option + '-tmPayments').text('-');
            $('#' + option + '-rBalance').text('-');
            $('#' + option + '-tObligation').text('-');
            $('#' + option + '-yCost').text('-');
        }
    }

    function removeOption(callback) {
        var optionToDelete = $(this).parent().parent().attr('id').split('-')[0];
        if (optionToDelete === 'option1') {
            ui.clearFirstOption(callback);
        } else {
            state.equipmentNextIndex -= Object.keys(state[optionToDelete].equipments).length;
            delete state[optionToDelete];

            $('#' + optionToDelete + '-container').off();
            $('#' + optionToDelete + '-container').remove();

            jcarousel.carouselRateCards();
            jcarousel.refreshCarouselItems();

            var index = $('#options-container').find('.rate-card-col').length;

            ui.moveButtonByIndex(index, false);

            if (optionToDelete === 'option2' && state['option3'] !== undefined) {
               
                ui.deleteSecondOption(callback);

                $('#option2-remove').on('click', function () {
                    removeOption.call(this, callback);
                });

                optionSetup('option2', callback);
            }
        }
    }

    var addOption = function (callback) {
        var index = $('#options-container').find('.rate-card-col').length;
        var secondIndex = index + 1;

        var optionToCopy = 'option' + index;
        var newOption = 'option' + secondIndex;

        ui.moveButtonByIndex(index, true);

        var container = $('#option' + index + '-container')
            .html()
            .replace("Option " + index, "Option " + secondIndex);

        var template = $.parseHTML(container);

        $(template).find('[id^="' + optionToCopy + '-"]').each(function () {
            $(this).attr('id', $(this).attr('id').replace(optionToCopy, newOption));
        });

        recalculateEquipmentId(template, optionToCopy, newOption);

        $(template).find('.calculator-remove').attr('id', newOption + '-remove');

        state[newOption] = $.extend(true, {}, state[optionToCopy]);

        var header = $(template).find('h2').text();
        $(header).text($(header).text().replace('Option ' + index, 'Option ' + secondIndex));

        var optionContainer = $('<li class="rate-card-col"></li>');
        optionContainer.attr('id', newOption + '-container');
        optionContainer.append(template);
        $('#options-container').append(optionContainer);

        $('#' + newOption + '-container')
            .find('.add-equip-link')
            .find('svg')
            .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-add-app"></use>');

        $('#' + newOption + '-container')
            .find('.clear-input')
            .find('svg')
            .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use>');

        toggleClearInputIcon($('#' + newOption + '-container').find('.control-group input, .control-group textarea'));

        $('#option' + secondIndex + '-remove').on('click', function () {
            removeOption.call(this, callback);
        });

        if (state[newOption].downPayment !== 0) {
            $('#' + newOption + '-downPayment').val(state[newOption].downPayment);
        }

        var currentPlan = constants.rateCards.filter(function(plan) { return plan.id === state[newOption].plan; })[0].name;
        $('#' + newOption + '-plan').val(currentPlan);

        var amortDropdownValue = state[newOption].LoanTerm  + '/' + state[newOption].AmortizationTerm;
        $('#' + newOption + '-amortDropdown').val(amortDropdownValue);

        if (state[newOption].plan === 2) {
            $('#' + newOption + '-deferralDropdown').val(state[newOption].DeferralPeriod);
        }

        if (state[newOption].plan === 3) {
            $('#' + newOption + '-customLoanTerm').val(state[newOption].LoanTerm);
            $('#' + newOption + '-customAmortTerm').val(state[newOption].AmortizationTerm);
            $('#' + newOption + '-customCRate').val(state[newOption].CustomerRate);
            $('#' + newOption + '-customYCostVal').val(state[newOption].DealerCost);
            $('#' + newOption + '-customAFee').val(state[newOption].AdminFee);
        }

        var indexes = Object.keys(state[optionToCopy].equipments).map(function (k) {
            return state[optionToCopy].equipments[k].id;
        });

        $.grep(indexes, function (id) {
            $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Cost').on('change', setters.setEquipmentCost(newOption, callback));
            $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Cost').val(state[newOption].equipments[id].cost);

            $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Description').on('change', setters.setEquipmentDescription(newOption));
            $('#Equipment_NewEquipment_' + id + '__Description').val(state[newOption].equipments[id].description);

            $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Type').on('change', setters.setEquipmentType(newOption));
            $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Type').val(state[newOption].equipments[id].type);

            if (id > 0) {
                $('#' + newOption + '-equipment-remove-' + id).on('click', function (e) {
                    var id = e.target.id;
                    id = id.substr(id.lastIndexOf('-') + 1);
                    recalculateEquipmentIndex(newOption, id);

                    setters.removeEquipment(newOption, id, callback);
                });
            }
        });

        jcarousel.refreshCarouselItems();
        jcarousel.carouselRateCards();

        $('.jcarousel').jcarousel('scroll', '+=1');

        optionSetup(newOption, callback);
    }


    function toggleClickInp(inp) {
        if (inp.val().length !== 0) {
            inp.siblings('.clear-input').css('display', 'block');
        } else {
            inp.siblings('.clear-input').hide();
        }
    }

    /**
    * Show/hide notification and disable dropdown option depending on totalAmountFinanced option
    * @param {string} option - name of the cards [FixedRate,Deferral,NoInterst]
    * @param {number} totalAmountFinanced - total cash value
    * @returns {} 
    */
    function renderDropdownValues(option, totalAmountFinanced) {
        var totalCash = constants.minimumLoanValue;

        if (totalAmountFinanced > constants.minimumLoanValue) {
            totalCash = totalAmountFinanced.toFixed(2);
        }

        var dropdown = $('#' + option + '-amortDropdown')[0];
        if (!dropdown || !dropdown.options) return;

        var options = dropdown.options;

        if (+totalCash >= constants.totalAmountFinancedFor180amortTerm) {
            toggleDisabledAttributeOnOption(option, options, false);
        } else {
            toggleDisabledAttributeOnOption(option, options, true);
        }
    }

    /**
     * depends on totalAmountfinanced value disable/enable options 
     * values of Loan/Amortization dropdown
     * @param {Array<>} options - array of available options for dropdown
     * @param {boolean} isDisable - disable/enable option and show/hide tooltip
     * @returns {} 
     */
    function toggleDisabledAttributeOnOption(option, options, isDisable) {
        if (!options.length) return;

        $.each(options, function (i) {
            var amortValue = +options[i].innerText.split('/')[1];
            if (amortValue >= constants.amortizationValueToDisable) {

                // if we selected max value of dropdown and totalAmountFinanced is lower then constants.amortizationValueToDisable
                // just select first option in dropdown
                if (options.selectedIndex === i && isDisable) {
                    $('#' + option + '-amortDropdown').val(options[0].value);
                }

                var opt = $(options[i]);
                opt.attr('disabled', isDisable);
                isDisable ? opt.addClass('disabled-opt') : opt.removeClass('disabled-opt');
            }
        });
    }

    var initValidation = function initValidation(option) {
        $('#' + option + '-downPayment').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            messages: {
                regex: translations.downPaymentInvalidFormat
            }
        });

        $('#' + option + '-customYCostVal').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            number: true,
            min: 0,
            messages: {
                regex: translations.yourCostFormat
            }
        });

        $('#' + option + '-customCRate').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$/,
            min: 0,
            number: true,
            messages: {
                regex: translations.customerRateFormat
            }
        });

        $('#' + option + '-customAFee').rules('add', {
            regex: /(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$/,
            number: true,
            min: 0,
            messages: {
                regex: translations.adminFeeFormat
            }
        });

        $('#' + option + '-customAmortTerm').rules('add', {
            regex: /^[1-9]\d{0,2}?$/,
            min: 1,
            max: 999,
            messages: {
                regex: translations.amortTermFormat,
                max: translations.amortTermMax
            }
        });

        $('#' + option + '-customLoanTerm').rules('add', {
            regex: /^[1-9]\d{0,2}?$/,
            min: 1,
            max: 999,
            messages: {
                regex: translations.loanTermFormat,
                max: translations.loanTermMax
            }
        });
    };

    return {
        addOption: addOption,
        optionSetup: optionSetup,
        renderOption: renderOption
    }
});
