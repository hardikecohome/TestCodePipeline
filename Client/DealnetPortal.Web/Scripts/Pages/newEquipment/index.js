module.exports('new-equipment',
    function(require) {
        var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
        var submitRateCard = require('rate-cards').submitRateCard;
        var setters = require('value-setters');
        var equipment = require('equipment');
        var validateOnSelect = require('custom-rate-card').validateOnSelect;
        var submitCustomRateCard = require('custom-rate-card').submitCustomRateCard;
        var toggleDisableClassOnInputs = require('custom-rate-card').toggleDisableClassOnInputs;
        var rateCardBlock = require('rate-cards-ui');
        var state = require('state').state;
        var constants = require('state').constants;

        var submitForm = function (event) {
            $('#equipment-form').valid();
            var agreementType = $("#typeOfAgreementSelect").find(":selected").val();
            if (agreementType === "0") {
                var rateCard = $('.checked');
                if (rateCard.length === 0 && !state.onlyCustomRateCard) {
                    event.preventDefault();
                } else {
                    var option = rateCard.find('#hidden-option').text();

                    if (state.onlyCustomRateCard) {
                        option = 'Custom';
                    }

                    var monthPayment = Globalize.parseNumber($('#' + option + 'TMPayments').text().replace('$','').trim());
                    if (isNaN(monthPayment) || (monthPayment == 0)) {
                        event.preventDefault();
                        $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                    } else {
                        if (option === 'Custom') {
                            submitCustomRateCard(event, option);
                        } else {
                            $('#custom-rate-card').clearErrors();
                            toggleCustomRateCard();
                            submitRateCard(option);
                        }
                        $('#equipment-form').submit();
                    }
                }
            } else {
                var monthPayment = Globalize.parseNumber($("#rentalTMPayment").text());
                if (isNaN(monthPayment) || (monthPayment == 0)) {
                    event.preventDefault();
                    $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                }
            }
        }

        var toggleRateCardBlock = function() {
            rateCardBlock.toggle($('#rateCardsBlock').is('.closed'));
            toggleDisableClassOnInputs(false);
        }

        var onRateCardSelect = function () {
            recalculateValuesAndRender();
            var option = $(this).parent().find('#hidden-option').text();
            if (option === 'Custom') {
                var isValid = validateOnSelect.call(this);

                if (isValid) {
                    rateCardBlock.hide();
                } else {
                    if (!$("#submit").hasClass('disabled')) {
                        $('#submit').addClass('disabled');
                        $('#submit').parent().popover();
                    }
                }
            } else {
                $('#custom-rate-card').clearErrors();
                toggleRateCardBlock();
                rateCardBlock.hide();
            }
        }

        function toggleCustomRateCard() {
            var isRental = $('#typeOfAgreementSelect').val() != 0;
            var option = $('.checked > #hidden-option').text();
            toggleDisableClassOnInputs(isRental || option !== 'Custom' && option !== '');
        }

        function onAmortizationDropdownChange(option) {
            $('#' + option + 'AmortizationDropdown').change(function () {
                $(this).find('option:selected').removeAttr('selected');

                recalculateValuesAndRender();
            });
        }

        function assignDatepicker() {
            //var input = $('body').is('.ios-device') ? $(this).siblings('.div-datepicker') : $(this);
            inputDateFocus(input);
            input.datepicker({
                yearRange: '1900:2200',
                minDate: new Date()
            });
        }

        $('.date-input').each(assignDatepicker);
        $.validator.addMethod(
            "date",
            function (value, element) {
                var minDate = Date.parse("1900-01-01");
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

        // submit
        $('#submit').on('click', submitForm);

        // handlers
        constants.rateCards.forEach(function (option) { onAmortizationDropdownChange(option.name) });

        $('#addEquipment').on('click', equipment.addEquipment);
        $('#addExistingEqipment').on('click', equipment.addExistingEquipment);

        $('#loanRateCardToggle').on('click', toggleRateCardBlock);
        $('#downPayment').on('change', setters.setDownPayment);
        $('#typeOfAgreementSelect').on('change', setters.setAgreement).on('change', toggleCustomRateCard);

        $('#total-monthly-payment').on('change', setters.setRentalMPayment);
        $('.btn-select-card').on('click', rateCardBlock.highlightCard);
        $('.btn-select-card').on('click', onRateCardSelect);

        // deferral
        $('#deferralLATerm').on('change', function(e) {
            var loanAmort = e.target.value.split('/');
            setters.setLoanAmortTerm('deferral')(loanAmort[0], loanAmort[1]);
        });

        $('#DeferralPeriodDropdown').on('change', setters.setDeferralPeriod('Deferral'));

        var agreementType = $("#typeOfAgreementSelect").find(":selected").val();
        state.agreementType = Number(agreementType);
    });
