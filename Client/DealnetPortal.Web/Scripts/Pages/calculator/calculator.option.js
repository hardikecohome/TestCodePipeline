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

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

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
            '-yCost': 'yourCost'
        },
        totalPriceFields: {
            '-totalEquipmentPrice': 'equipmentSum',
            '-tax': 'tax',
            '-totalPrice': 'totalPrice'
        },
        displaySectionFields: {
            'displayCustomerRate': 'CustomerRate',
            'displayTFinanced': 'totalAmountFinanced',
            'displayMPayment': 'monthlyPayment'
        }
    }

    function _optionSetup(option, callback) {

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

                optionSetup('option2', callback);
            }
        }
    }

    function _addOption(callback) {
        if (!$('#province-form').valid()) {
            return;
        }

        var index = $('#options-container').find('.rate-card-col').length;

        var optionToCopy = 'option' + index;

        if (!$('#' + optionToCopy + '-container > form').valid()) {
            return;
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

        var currentPlan = constants.rateCards.filter(function(plan) { return plan.id === state[newOption].plan; })[0]
            .name;
        $('#' + newOption + '-plan').val(currentPlan);

        var amortDropdownValue = state[newOption].LoanTerm + '/' + state[newOption].AmortizationTerm;
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

    function _renderOption(options) {
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
            var eSumData = rateCardsCalculator.calculateTotalPrice(state[option].equipments, state.downPayment, state.tax);

            rateCardsRenderEngine.renderTotalPrice(eSumData);

            var rateCard = rateCardsCalculator.filterRateCard(state[option].plan);

            if (rateCard !== null && rateCard !== undefined) {
                $.extend(state[option], rateCard);
                rateCardsRenderEngine.renderAfterFiltration(state[option].plan, { deferralPeriod: state[option].DeferralPeriod, adminFee: state[option].AdminFee, dealerCost: state[option].DealerCost, customerRate: state[option].CustomerRate });
            }

            if (option.name !== 'Custom') {
                rateCardsRenderEngine.renderDropdownValues(state[option].plan);
            }

            var data = rateCardsCalculator.calculateValuesForRender($.extend({}, idToValue(state)(state[option])));
            var selectedRateCard = $('#rateCardsBlock').find('div.checked').length > 0
                ? $('#rateCardsBlock').find('div.checked').find('#hidden-option').text()
                : '';

            rateCardsRenderEngine.renderOption(option.name, selectedRateCard, data);
        });
    }

    function _initHandlers() {
        $('#province-tax-rate').on('change', setTax(_renderOption));
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
