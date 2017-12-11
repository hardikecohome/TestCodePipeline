module.exports('newEquipment.index',
    function (require) {
        var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
        var submitRateCard = require('rate-cards').submitRateCard;
        var setters = require('value-setters');
        var equipment = require('equipment');
        var rateCardsInit = require('rate-cards-init');
        var validateOnSelect = require('custom-rate-card').validateOnSelect;
        var customRateCardInit = require('custom-rate-card').init;
        var submitCustomRateCard = require('custom-rate-card').submitCustomRateCard;
        var toggleDisableClassOnInputs = require('custom-rate-card').toggleDisableClassOnInputs;
        var rateCardBlock = require('rate-cards-ui');
        var state = require('state').state;
        var constants = require('state').constants;

        var settings = Object.freeze({
            customRateCardName: 'Custom',
            formId: '#equipment-form',
            agreementTypeId: '#typeOfAgreementSelect',
            submitButtonId: '#submit',
            rateCardTypeId: '#hidden-option',
            equipmentValidationMessageId: '#new-equipment-validation-message',
            addEquipmentId: '#addEquipment',
            addExistingEquipmentId: '#addExistingEqipment',
            toggleRateCardBlockId: '#loanRateCardToggle',
            totalMonthlyPaymentId: '#total-monthly-payment',
            downPaymentId: '#downPayment',
            selectRateCardButtonClass: '.btn-select-card',
            deferralDropdownId: '#DeferralPeriodDropdown',
            deferralTermId: '#deferralLATerm',
            expiredRateCardWarningId: '#expired-rate-card-warning',
            customRateCardId: '#custom-rate-card',
            rateCardBlockId: '#rateCardsBlock',
            rentalMonthlyPaymentId: '#rentalTMPayment',
            applicationType: {
                'loanApplication': '0',
                'rentalApplicationHwt': '1',
                'rentalApplication': '2'
            }
        });

        var init = function (id, cards, onlyCustomRateCard) {
            var agreementType = $(settings.agreementTypeId).find(":selected").val();
            state.agreementType = Number(agreementType);
            _initHandlers();
            _initDatepickers();

            equipment.init();
            rateCardsInit.init(id, cards, onlyCustomRateCard);
            customRateCardInit();
            rateCardBlock.init();

            if (agreementType === settings.applicationType.loanApplication) {
                recalculateValuesAndRender();
            } else {
                recalculateAndRenderRentalValues();
            }
        }

        function _submitForm (event) {
            $(settings.formId).valid();
            var agreementType = $(settings.agreementTypeId).find(":selected").val();
            var rateCard = $('.checked');
            var option = state.onlyCustomRateCard ? settings.customRateCardName : rateCard.find(settings.rateCardTypeId).text();
            var monthPayment = agreementType === settings.applicationType.loanApplication ?
                Globalize.parseNumber($('#' + option + 'TMPayments').text().replace('$', '').trim()) :
                Globalize.parseNumber($(settings.rentalMonthlyPaymentId).text());

            if (rateCard.length === 0 && !state.onlyCustomRateCard) {
                event.preventDefault();
                return;
            }

            if (isNaN(monthPayment) || (monthPayment <= 0)) {
                event.preventDefault();
                $(settings.equipmentValidationMessageId).text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                return;
            }

            if (option === settings.customRateCardName) {
                submitCustomRateCard(event, option);
            } else {
                $(settings.customRateCardId).clearErrors();
                _toggleCustomRateCard();
                submitRateCard(option);
            }

            $(settings.formId).submit();
        }

        function _toggleRateCardBlock() {
            rateCardBlock.toggle($(settings.rateCardBlockId).is('.closed'));
            toggleDisableClassOnInputs(false);
        }

        function _onRateCardSelect() {
            recalculateValuesAndRender();
            var option = $(this).parent().find(settings.rateCardTypeId).text();
            if ($(settings.expiredRateCardWarningId).is(':visible')) {
                $(settings.expiredRateCardWarningId).addClass('hidden');
            }

            if (option === settings.customRateCardName) {
                var isValid = validateOnSelect.call(this);

                if (isValid) {
                    rateCardBlock.hide();
                } else {
                    if (!$(settings.submitButtonId).hasClass('disabled')) {
                        $(settings.submitButtonId).addClass('disabled');
                        $(settings.submitButtonId).parent().popover();
                    }
                }
            } else {
                $(settings.customRateCardId).clearErrors();
                _toggleRateCardBlock();
                rateCardBlock.hide();
            }
        }

        function _toggleCustomRateCard () {
            var isRental = +$(settings.agreementTypeId).val() !== 0;
            var option = $('.checked > ' + settings.rateCardTypeId).text();
            toggleDisableClassOnInputs(isRental || option !== settings.customRateCardName && option !== '');
        }

        function _initHandlers () {
            $(settings.submitButtonId).on('click', _submitForm);
            constants.rateCards.forEach(function (option) { $('#' + option.name + 'AmortizationDropdown').on('change', recalculateValuesAndRender) });
            $(settings.addEquipmentId).on('click', equipment.addEquipment);
            $(settings.addExistingEquipmentId).on('click', equipment.addExistingEquipment);
            $(settings.toggleRateCardBlockId).on('click', _toggleRateCardBlock);
            $(settings.downPaymentId).on('change', setters.setDownPayment);
            $(settings.agreementTypeId).on('change', setters.setAgreement).on('change', _toggleCustomRateCard);
            $(settings.totalMonthlyPaymentId).on('change', setters.setRentalMPayment);
            $(settings.selectRateCardButtonClass).on('click', rateCardBlock.highlightCard);
            $(settings.selectRateCardButtonClass).on('click', _onRateCardSelect);
            $(settings.deferralTermId).on('change', setters.setLoanAmortTerm('deferral'));
            $(settings.deferralDropdownId).on('change', setters.setDeferralPeriod('deferral'));
        }

        function _initDatepickers () {
            var datepickerOptions = {
                yearRange: '1900:2200',
                minDate: new Date()
            };

            $('.date-input').each(function (index, input) {
                assignDatepicker(input, datepickerOptions);
            });

            $.validator.addMethod("date",
                function (value, element) {
                    var minDate = new Date("1900-01-01");
                    var valueEntered = Date.parseExact(value, "M/d/yyyy");
                    if (!valueEntered) {
                        return false;
                    }
                    if (valueEntered < minDate) {
                        return false;
                    }
                    return true;
                },
                translations['EnterValidDate']
            );
        }

        return { init: init };
    });
