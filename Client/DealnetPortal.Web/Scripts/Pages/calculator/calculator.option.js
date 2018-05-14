module.exports('calculator.option', function (require) {
    var setters = require('calculator-value-setters');
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var ui = require('calculator-ui');
    var recalculateEquipmentIndex = require('calculator-conversion').recalculateEquipmentIndex;
    var recalculateEquipmentId = require('calculator-conversion').recalculateEquipmentId;
    var resetValidation = require('calculator-conversion').resetValidation;
    var jcarousel = require('calculator.jcarousel');
    var validation = require('calculator.validation');
    var rateCardsCalculator = require('rateCards.index');
    var rateCardsRenderEngine = require('rateCards.render');

    var settings = {
        rateCardFields: {
            '-mPayment': 'monthlyPayment',
            '-cBorrowing': 'costOfBorrowing',
            '-taFinanced': 'totalAmountFinanced',
            '-tmPayments': 'totalMonthlyPayments',
            '-rBalance': 'residualBalance',
            '-tObligation': 'totalObligation',
            '-yCost': 'yourCost',
            '-aFee': 'adminFee'
        },
        totalPriceFields: {
            '-totalEquipmentPrice': 'equipmentSum',
            '-tax': 'tax',
            '-totalPrice': 'totalPrice'
        },
        displaySectionFields: {
            '-cRate': 'CustomerRate'
        },
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm'],
        reductionCards: ['FixedRate', 'Deferral']
    }

    function _optionSetup(option, callback) {

        $('#' + option + '-addEquipment').on('click', ui.addEquipment(option, callback));

        $('#' + option + '-downPayment').on('change', setters.setDownPayment(option, callback));
        $('#' + option + '-plan').on('change', setters.setRateCardPlan(option, callback));
        $('#' + option + '-amortDropdown').on('change', setters.setLoanAmortTerm(option, callback));
        $('#' + option + '-deferralDropdown').on('change', setters.setDeferralPeriod(option, callback));
        $('#' + option + '-aFeeOptionsHolder').find('.custom-radio').on('click', _setAdminFeeCoveredBy(option, callback));
        $('#' + option + '-programDropdown').on('change', setters.setProgram(option, callback));
        $('#' + option + '-reduction').on('change', setters.setReductionCost(option, callback));

        if (option === 'option1') {
            var nextIndex = state[option].equipmentNextIndex;

            state[option].equipments[nextIndex] = { id: nextIndex.toString(), cost: '', description: '' };
            state[option].equipments[nextIndex].type = $('#Equipment_NewEquipment_' + nextIndex + '__Type').val();

            var container = $('#option1-equipment-0');

            container.find('input, select, textarea').each(function() {
                $(this).attr('id',
                    $(this).attr('id').replace('Equipment_NewEquipment_0', 'Equipment_NewEquipment_option1_0'));
                $(this).attr('name',
                    $(this).attr('name').replace('Equipment.NewEquipment[0', 'Equipment.NewEquipment[option1_0'));
            });

            container.find('label').each(function() {
                $(this).attr('for',
                    $(this).attr('for').replace('Equipment_NewEquipment_0', 'Equipment_NewEquipment_option1_0'));
            });

            container.find('span').each(function() {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor === null || valFor === undefined) {
                    return;
                }

                $(this).attr('data-valmsg-for',
                    valFor.replace('Equipment.NewEquipment[0', 'Equipment.NewEquipment[option1_0'));
            });

            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Cost')
                .on('change', setters.setEquipmentCost(option, callback));
            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Description')
                .on('change', setters.setEquipmentDescription(option));
            $('#Equipment_NewEquipment_option1_' + nextIndex + '__Type').on('change', setters.setEquipmentType(option));
            $('#option1-remove').on('click',
                function() {
                    _removeOption.call(this, callback);
                });

            state[option].equipmentNextIndex++;

            $('#' + option + '-plan').change();
        }

        resetValidation(option);

        validation.setValidationForOption(option);
    };

    function _setAdminFeeCoveredBy(option, callback) {
        return function() {
            var $this = $(this);
            $('#' + option + '-errorAdminFee').addClass('hidden');
            $('#' + option + '-aFeeOptionsHolder').find('.afee-is-covered').prop('checked', false);
            var $input = $this.find('input');
            var val = $input.val().toLowerCase() === 'true';

            $input.prop('checked', true);

            _toggleIsAdminFeeCovered(option, val);
            setters.setAdminFeeIsCovered(option, val, callback);
        }
    }

    function _toggleIsAdminFeeCovered(option, isCovered) {
        if (isCovered == null) return;

        $('#' + option + '-aFee').parent().removeClass('hidden');
        var text = isCovered ? translations.coveredByCustomer : translations.coveredByDealer;
        $('#' + option + '-aFeeHolder').text('(' + text + ')');
    }

    function _removeOption(callback) {
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
                    _removeOption.call(this, callback);
                });

                _optionSetup('option2', callback);
            }
        }
    }

    function _addOption(callback) {
        var index = $('#options-container').find('.rate-card-col').length;

        var optionToCopy = 'option' + index;

        if (!$('#' + optionToCopy + '-container > form').valid()) {
            return;
        }

        if ($('#' + optionToCopy + '-aFeeOptionsHolder').is(':visible') && !$('#' + optionToCopy + '-aFeeOptionsHolder').find('input:checked').length) {
            $('#' + optionToCopy + '-errorAdminFee').removeClass('hidden');
            return;
        } else {
            $('#' + optionToCopy + '-errorAdminFee').addClass('hidden');
        }

        var secondIndex = index + 1;
        var newOption = 'option' + secondIndex;

        ui.moveButtonByIndex(index, true);

        var container = $('#option' + index + '-container')
            .html()
            .replace("Option " + index, "Option " + secondIndex);

        var template = $.parseHTML(container);
        var $template = $(template);

        $template.find('[id^="' + optionToCopy + '-"]').each(function() {
            var $this = $(this);
            $this.attr('id', $this.attr('id').replace(optionToCopy, newOption));
        });

        $template.find('[name^="' + optionToCopy + '"]').each(function() {
            var $this = $(this);
            $this.attr('name', $this.attr('name').replace(optionToCopy, newOption));
        });

        $template.find('[data-valmsg-for^="' + optionToCopy + '"]').each(function() {
            var $this = $(this);
            var newValFor = $this.data('valmsg-for').replace(optionToCopy, newOption);
            $this.attr('data-valmsg-for', newValFor);
        });

        recalculateEquipmentId(template, optionToCopy, newOption);

        $template.find('.calculator-remove').attr('id', newOption + '-remove');

        state[newOption] = $.extend(true, {}, state[optionToCopy]);

        var header = $template.find('h2').text();
        $(header).text($(header).text().replace('Option ' + index, 'Option ' + secondIndex));

        var optionContainer = $('<li class="rate-card-col"></li>');
        optionContainer.attr('id', newOption + '-container');
        optionContainer.append(template);
        $('#options-container').append(optionContainer);

        if ($('#' + optionToCopy + '-aFeeOptionsHolder').find('input:radio:checked').length) {
            if (state[newOption].includeAdminFee) {
                $('#' + newOption + '-radioWillPay').prop('checked', true);
            } else {
                $('#' + newOption + '-radioNotPay').prop('checked', true);
            }
        };

        $('#' + newOption + '-container')
            .find('.add-equip-link')
            .find('svg')
            .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-add-app"></use>');

        $('#' + newOption + '-container')
            .find('.clear-input')
            .find('svg')
            .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use>');

        $('#option' + secondIndex + '-remove').on('click',
            function() {
                _removeOption.call(this, callback);
            });

        if (state[newOption].downPayment !== 0) {
            $('#' + newOption + '-downPayment').val(state[newOption].downPayment);
        }

        var currentPlan = constants.rateCards.filter(function(plan) { return plan.name === state[newOption].plan; })[0].name;
        $('#' + newOption + '-plan').val(currentPlan);
        $('#' + newOption + '-amortDropdown').val(state[newOption].AmortizationTerm);

        if (state[newOption].plan === 'Deferral') {
            $('#' + newOption + '-deferralDropdown').val(state[newOption].DeferralPeriod);
        }

        var indexes = Object.keys(state[optionToCopy].equipments).map(function(k) {
            return state[optionToCopy].equipments[k].id;
        });

        $.grep(indexes,
            function(id) {
                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Cost')
                    .on('change', setters.setEquipmentCost(newOption, callback));
                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Cost')
                    .val(state[newOption].equipments[id].cost);

                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Description')
                    .on('change', setters.setEquipmentDescription(newOption));
                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Description')
                    .val(state[newOption].equipments[id].description);

                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Type')
                    .on('change', setters.setEquipmentType(newOption));
                $('#Equipment_NewEquipment_' + newOption + '_' + id + '__Type')
                    .val(state[newOption].equipments[id].type);

                if (id > 0) {
                    $('#' + newOption + '-equipment-remove-' + id).on('click',
                        function(e) {
                            var id = e.target.id;
                            id = id.substr(id.lastIndexOf('-') + 1);
                            recalculateEquipmentIndex(newOption, id);

                            setters.removeEquipment(newOption, id, callback);
                        });
                }
            });

        ui.toggleClearInputIcon($('#' + newOption + '-container')
            .find('.control-group input, .control-group textarea'));

        jcarousel.refreshCarouselItems();
        jcarousel.carouselRateCards();

        $('.jcarousel').jcarousel('scroll', '+=1');

        _optionSetup(newOption, callback);
    };

    function _renderOption(options, resetDropdown) {
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
            var downPayment = +$('#' + option + '-downPayment').val();

            var eSumData = rateCardsCalculator.calculateTotalPrice(state[option].equipments, downPayment, state.tax);

            rateCardsRenderEngine.renderTotalPrice(option, eSumData);

            if (state[option].plan !== 'Custom') {
                if (resetDropdown !== undefined && resetDropdown === true) {
                    var e = document.getElementById(option + '-amortDropdown');
                    e.selectedIndex = -1;
                }
                if (state.programsAvailable) {
                    rateCardsRenderEngine.renderProgramDropdownValues({
                        rateCardPlan: state[option].plan,
                        standaloneOption: option,
                        totalAmountFinanced: rateCardsCalculator.getTotalAmountFinanced()
                    });
                    state[option].Program = $('#' + option + '-programDropdown option:selected').val();
                }

                rateCardsRenderEngine.renderDropdownValues({
                    rateCardPlan: state[option].plan,
                    standaloneOption: option,
                    totalAmountFinanced: rateCardsCalculator.getTotalAmountFinanced()
                });
                state[option].AmortizationTerm = +$('#' + option + '-amortDropdown option:selected').val();
            } else {
                _setAdminFeeByEquipmentSum(option, eSumData.totalPrice !== "-" ? eSumData.totalPrice : 0);
            }

            var rateCard = rateCardsCalculator.filterRateCard({ rateCardPlan: state[option].plan, standaloneOption: option });

            if (rateCard !== null && rateCard !== undefined) {
                if (!state[option].CustomerReduction || !state[option].InterestRateReduction) {
                    state[option].CustomerReduction = !state[option].CustomerReduction ? 0 : state[option].CustomerReduction;
                    state[option].InterestRateReduction = !state[option].InterestRateReduction ? 0 : state[option].InterestRateReduction;
                }

                $.extend(true, state[option], rateCard);

                if (settings.reductionCards.indexOf(state[option].plan) !== -1) {
                    _setReductionRates(option);
                }

                rateCardsRenderEngine.renderAfterFiltration(state[option].plan, { deferralPeriod: state[option].DeferralPeriod, adminFee: state[option].AdminFee, dealerCost: state[option].DealerCost, customerRate: state[option].CustomerRate });
            }

            var assignOption = $.extend(true, {}, state[option]);
            delete assignOption.plan;
            delete assignOption.equipments;

            var data = rateCardsCalculator.calculateValuesForRender($.extend({}, assignOption));
            var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
                ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
                : '';
            delete data.CustomerRiskGroup;
            delete data.Program;

            rateCardsRenderEngine.renderOption(option, selectedRateCard, data);
        });
    }

    function _setReductionRates(rateCardOption) {

        var taf = rateCardsCalculator.getTotalAmountFinanced({
            includeAdminFee: state[rateCardOption].includeAdminFee,
            AdminFee: state[rateCardOption].AdminFee
        });

        rateCardsRenderEngine.renderReductionDropdownValues({
            standaloneOption: rateCardOption,
            rateCardPlan: state[rateCardOption].plan,
            customerRate: state[rateCardOption].CustomerRate,
            totalAmountFinanced: taf
        });

        var reducedCustomerRate = state[rateCardOption].CustomerRate - state[rateCardOption].CustomerReduction;
        if (reducedCustomerRate < 0) {
            state[rateCardOption].ReductionId = null;
            state[rateCardOption].CustomerReduction = 0;
            state[rateCardOption].InterestRateReduction = 0;
        } else {
            state[rateCardOption].CustomerRate = reducedCustomerRate;
        }
    }

    function _setAdminFeeByEquipmentSum(option, eSum) {
        if($.isEmptyObject(state.customRateCardBoundaires)) return;

        var keys = Object.keys(state.customRateCardBoundaires);
        for (var i = 0; i < keys.length; i++) {

            var numbers = keys[i].split('-');
            var lowBound = +numbers[0];
            var highBound = +numbers[1];

            if (eSum <= lowBound || eSum >= highBound) {
                state[option].AdminFee = 0;
            }

            if (lowBound <= eSum && highBound >= eSum) {
                state[option].AdminFee  = +state.customRateCardBoundaires[keys[i]].adminFee;
                break;
            }
        }
    }

    function _initHandlers() {
        //$('#province-tax-rate').on('change', setters.setTax(_renderOption));
        $('.btn-add-calc-option').on('click', function () { _addOption(_renderOption); });
    }

    var init = function() {
        var name = 'option1';

        rateCardsRenderEngine.init(settings);
        _initHandlers();
        _optionSetup(name, _renderOption);
        _renderOption(name);
    }
  
    return {
        init: init
    };
});
