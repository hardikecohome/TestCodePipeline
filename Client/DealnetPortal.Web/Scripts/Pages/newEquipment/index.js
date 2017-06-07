module.exports('new-equipment',
    function(require) {
        var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
        var setters = require('value-setters');
        var equipment = require('equipment');

        var constants = require('state').constants;
        var state = require('state').state;

        // setters

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
                        $('#new-equipment-validation-message')
                            .text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                    } else {
                        if (option === 'Custom') {
                            if ($('#amortLoanTermError').is(':visible')) {
                                event.preventDefault();
                            }

                            var customSlicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);
                            $('#AmortizationTerm').val(state[option].AmortizationTerm);
                            $('#LoanTerm').val(state[option].LoanTerm);
                            $('#CustomerRate').val(state[option].CustomerRate);
                            $('#AdminFee').val(state[option].AdminFee);
                            $('#total-monthly-payment').val(customSlicedTotalMPayment);
                            if (state[option].DeferralPeriod === '')
                            $('#LoanDeferralType').val(state[option].DeferralPeriod);
                            $('#SelectedRateCardId').val(0);
                        } else {

                            if (option === 'Deferral') {
                                $('#LoanDeferralType').val($('#DeferralPeriodDropdown').val());
                            } else {
                                $('#LoanDeferralType').val('0');
                            }

                            //remove percentage symbol
                            var amortizationTerm = $('#' + option + 'AmortizationDropdown').val();
                            var loanTerm = amortizationTerm;

                            var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
                            var slicedAdminFee = $('#' + option + 'AFee').text().substring(1);
                            var slicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);
                            var contractId = $('#ContractId').val();
                            var cards = sessionStorage.getItem(contractId + option);
                            if (cards !== null) {
                                var cardType = $.grep(constants.rateCards, function (c) { return c.name === option; })[0].id;

                                var filtred = $.grep($.parseJSON(cards),
                                    function (v) {
                                        return v.CardType === cardType
                                            && v.AmortizationTerm === Number(amortizationTerm)
                                            && v.AdminFee === Number(slicedAdminFee)
                                            && v.CustomerRate === Number(slicedCustomerRate);
                                    })[0];

                                if (filtred !== undefined) {
                                    $('#SelectedRateCardId').val(filtred.Id);
                                }
                            }
                            $('#AmortizationTerm').val(amortizationTerm);
                            $('#LoanTerm').val(loanTerm);
                            $('#total-monthly-payment').val(slicedTotalMPayment);
                            $('#CustomerRate').val(slicedCustomerRate);
                            $('#AdminFee').val(slicedAdminFee);
                        }
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

        // submit
        $('#equipment-form').submit(submitForm);

        // handlers
        $('#addEquipment').on('click', equipment.addEquipment);
        $('#downPayment').on('change', setters.setDownPayment);
        $('#typeOfAgreementSelect').on('change', setters.setAgreement);
        $('#total-monthly-payment').on('change', setters.setRentalMPayment);

        $('.btn-select-card').on('click', function () {
            $(this).parents('.rate-card').addClass('checked').siblings().removeClass('checked');
            recalculateValuesAndRender([], false);
            var option = $(this).parent().find('#hidden-option').text();
            if (option === 'Custom' && !validateOnSelect()) {
              var panel = $(this).parent();
              $.grep(panel.find('input[type="text"]'), function (inp) {
                var input = $(inp);
                if (input.val() === '' && !(input.is('#CustomYCostVal') || input.is('#CustomAFee'))) {
                    input.addClass('input-validation-error');
                }
               })
            } else {
                $('#rateCardsBlock').hide('slow', function () {
                    $('#loanRateCardToggle').find('i.glyphicon')
                        .removeClass('glyphicon-chevron-down')
                        .addClass('glyphicon-chevron-right');
                });
            }
        });

        // deferral
        $('#deferralLATerm').on('change', function(e) {
            var loanAmort = e.target.value.split('/');
            setters.setLoanAmortTerm('deferral')(loanAmort[0], loanAmort[1]);
        });

        $('#DeferralPeriodDropdown').on('change', setters.setDeferralPeriod('Deferral'));

        var agreementType = $("#typeOfAgreementSelect").find(":selected").val();
        state.agreementType = Number(agreementType);
    });