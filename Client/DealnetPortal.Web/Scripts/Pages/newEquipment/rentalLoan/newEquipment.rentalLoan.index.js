﻿module.exports('newEquipment.rentalLoan.index',
    function (require) {
        var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
        var recalculateAndRenderRentalValues = require('newEquipment.rental').recalculateAndRenderRentalValues;
        var submitRateCard = require('rate-cards').submitRateCard;
        var rateCardCalculationInit = require('rate-cards').init;
        var setters = require('value-setters');
        var equipment = require('equipment');
        var rateCardsInit = require('rate-cards-init');
        var validateCustomCard = require('custom-rate-card').validateCustomCard;
        var customRateCardInit = require('custom-rate-card').init;
        var submitCustomRateCard = require('custom-rate-card').submitCustomRateCard;
        var toggleDisableClassOnInputs = require('custom-rate-card').toggleDisableClassOnInputs;
        var rateCardBlock = require('rate-cards-ui');
        var state = require('state').state;
        var constants = require('state').constants;

        var navigateToStep = require('navigateToStep');
        var datepicker = require('datepicker');

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
            dealProvinceId: '#DealProvince',
            coveredByCustomerId: '#isCoveredByCustomer',
            passAdminFeeId: '#isPassAdminFee',
            adminFeeSectionId: '#admin-fee-section',
            isCustomerFoundInCreditBureauId: '#isCustomerFoundInCreditBureau',
            applicationType: {
                'loanApplication': '0',
                'rentalApplicationHwt': '1',
                'rentalApplication': '2'
            }
        });

        /**
         * Entry Point
         * @param {number} id - contract id 
         * @param {Object<>} cards - list of available rate cards for the dealer 
         * @param {boolean} onlyCustomRateCard - flag indicates that we have only one card 
         * @returns {void} 
         */
        var init = function (id, cards, onlyCustomRateCard, bill59Equipment) {
            var isOnlyLoan = $(settings.dealProvinceId).val().toLowerCase() == 'qc';
            
            if (isOnlyLoan) {
                if ($(settings.agreementTypeId).find(":selected").val() !== settings.applicationType.loanApplication) {
                    $(settings.agreementTypeId).val(settings.applicationType.loanApplication);
                }
                $(settings.agreementTypeId).attr('disabled', true);
            }

            var agreementType = $(settings.agreementTypeId).find(":selected").val();
            state.agreementType = Number(agreementType);
            state.isDisplayAdminFee = $(settings.passAdminFeeId).val().toLowerCase() === 'true';
            state.isCustomerFoundInCreditBureau = $(settings.isCustomerFoundInCreditBureauId).val().toLowerCase() === 'true';

            if (state.isDisplayAdminFee) {
                $(settings.adminFeeSectionId).removeClass('hidden');
                state.isCoveredByCustomer = $(settings.coveredByCustomerId).val() !== ''
                    ? $(settings.coveredByCustomerId).val().toLowerCase() === 'true'
                    : undefined;
            } else {
                state.isCoveredByCustomer = undefined;
            }

            state.bill59Equipment = bill59Equipment;
            state.isOntario = $(settings.dealProvinceId).val().toLowerCase() == 'on';

            _initHandlers();
            _initDatepickers();

            setters.init({
                isClarity: false,
                recalculateValuesAndRender: recalculateValuesAndRender,
                recalculateAndRenderRentalValues: recalculateAndRenderRentalValues
            });
            equipment.init({
                isClarity: false,
                recalculateValuesAndRender: recalculateValuesAndRender,
                recalculateAndRenderRentalValues: recalculateAndRenderRentalValues
            });

            rateCardsInit.init(id, cards, onlyCustomRateCard);
            customRateCardInit();
            rateCardCalculationInit();
            rateCardBlock.init();
            rateCardBlock.toggleIsAdminFeeCovered($(settings.coveredByCustomerId).val() === '' ? null : state.isCoveredByCustomer);

            if (agreementType === settings.applicationType.loanApplication) {
                recalculateValuesAndRender();
            } else {
                recalculateAndRenderRentalValues();
            }

            $('#steps .step-item[data-warning="true"]').on('click', function () {
                if ($(this).attr('href')) {
                    navigateToStep($(this));
                }
                return false;
            });
        };

        function _submitForm(event) {
            $(settings.formId).valid();
            var agreementType = $(settings.agreementTypeId).find(":selected").val();
            var rateCard = $('.checked');
            var option = state.onlyCustomRateCard ? settings.customRateCardName : rateCard.find(settings.rateCardTypeId).text();
            var monthPayment = agreementType === settings.applicationType.loanApplication ?
                Globalize.parseNumber($('#' + option + 'TMPayments').text().replace('$', '').trim()) :
                Globalize.parseNumber($(settings.rentalMonthlyPaymentId).text());

            if (rateCard.length === 0 && !state.onlyCustomRateCard && agreementType === settings.applicationType.loanApplication) {
                event.preventDefault();
                return;
            }

            if (isNaN(monthPayment) || monthPayment <= 0) {
                event.preventDefault();
                $(settings.equipmentValidationMessageId).text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                return;
            }

            if (agreementType === settings.applicationType.loanApplication) {
                if (option === settings.customRateCardName) {
                    submitCustomRateCard(event, option);
                } else {
                    $(settings.customRateCardId).clearErrors();
                    _toggleCustomRateCard();
                    submitRateCard(option);
                }
            }
            if ($('#sales-rep-types').is(':visible')) {
                if (!$('#sales-rep-types').find('input[type=checkbox]:checked').length) {
                    event.preventDefault();
                    _toggleSalesRepErrors(true);
                }
            }

            if (!$(settings.formId).valid()) {
                event.preventDefault();
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
                var isValid = validateCustomCard.call(this);

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

        function _toggleCustomRateCard() {
            var isRental = +$(settings.agreementTypeId).val() !== 0;
            var option = $('.checked > ' + settings.rateCardTypeId).text();
            toggleDisableClassOnInputs(isRental || option !== settings.customRateCardName && option !== '');
        }

        function _initHandlers() {
            $(settings.submitButtonId).on('click', _submitForm);
            constants.rateCards.forEach(function (option) {
                $('#' + option.name + '-amortDropdown').on('change', recalculateValuesAndRender);
            });
            $(settings.addEquipmentId).on('click', equipment.addEquipment);
            $(settings.addExistingEquipmentId).on('click', equipment.addExistingEquipment);
            $(settings.toggleRateCardBlockId).on('click', _toggleRateCardBlock);
            $(settings.downPaymentId).on('change', setters.setDownPayment);
            $(settings.agreementTypeId).on('change', setters.setAgreement).on('change', _toggleCustomRateCard);
            $(settings.totalMonthlyPaymentId).on('change', setters.setRentalMPayment);
            $(settings.selectRateCardButtonClass).on('click', rateCardBlock.highlightCard);
            $(settings.selectRateCardButtonClass).on('click', _onRateCardSelect);
            $(settings.deferralTermId).on('change', setters.setLoanAmortTerm('Deferral'));
            $(settings.deferralDropdownId).on('change', setters.setDeferralPeriod('Deferral'));

            $('#initiated-contract-checkbox').on('click', function (e) {
                $('#initiated-contract').val(e.target.checked); _toggleSalesRepErrors(false);
            });
            $('#negotiated-agreement-checkbox').on('click', function (e) {
                $('#negotiated-agreement').val(e.target.checked); _toggleSalesRepErrors(false);
            });
            $('#concluded-agreement-checkbox').on('click', function (e) {
                $('#concluded-agreement').val(e.target.checked); _toggleSalesRepErrors(false);
            });
            $('.custom-radio').on('click', _setAdminFeeCoveredBy);
        }

        function _setAdminFeeCoveredBy() {
            var $this = $(this);

            $('.afee-is-covered').prop('checked', false);
            var $input = $this.find('input');
            var val = $input.val().toLowerCase() === 'true';
            $(settings.coveredByCustomerId).val(val);
            rateCardBlock.toggleIsAdminFeeCovered(val);
            $input.prop('checked', true);
            setters.setAdminFeeIsCovered(val);
        }


        function _toggleSalesRepErrors(show) {
            $.each($('#sales-rep-types').find('span.checkbox-icon'), function(i, el) {
                var $el = $(el);
                show ? $el.addClass('checkbox-error') : $el.removeClass('checkbox-error');
            });

            show ? $('#error-message').removeClass('hidden') : $('#error-message').addClass('hidden');
        }

        function _initDatepickers() {
            var datepickerOptions = {
                yearRange: '1900:2200',
                minDate: new Date()
            };

            $('.date-input').each(function (index, input) {
                datepicker.assignDatepicker(input, datepickerOptions);
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

        return {
            init: init
        };
    });