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

        var onRateCardSelect = function() {
            recalculateValuesAndRender([], false);
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
                toggleDisableClassOnInputs(true);
                rateCardBlock.hide();
            }
        }

        var submitForm = function (event) {
            var agreementType = $("#typeOfAgreementSelect").find(":selected").val();
            if (agreementType === "0") {
                var rateCard = $('.checked');
                if (rateCard.length === 0) {
                    event.preventDefault();
                } else {
                    var option = rateCard.find('#hidden-option').text();
                    var monthPayment = Globalize.parseNumber($('#' + option + 'TMPayments').text().substring(1));
                    if (isNaN(monthPayment) || (monthPayment == 0)) {
                        event.preventDefault();
                        $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                    } else {
                        option === 'Custom' ? submitCustomRateCard(event, option) : submitRateCard(option);
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

        // submit
        $('#equipment-form').submit(submitForm);

        // handlers
        $('#addEquipment').on('click', equipment.addEquipment);
        $('#loanRateCardToggle').on('click', toggleRateCardBlock);

        $('#downPayment').on('change', setters.setDownPayment);
        $('#typeOfAgreementSelect').on('change', setters.setAgreement);
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
