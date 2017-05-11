module.exports('new-equipment',
    function(require) {
        var equipmentTemplateFactory = require('equipment-template');
        var state = require('rate-cards').state;
        var rateCards = require('rate-cards').rateCards;
        var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;
        var recalculateAndRenderRentalValues = require('rate-cards').recalculateAndRenderRentalValues;
        var recalculateRentalTaxAndPrice = require('rate-cards').recalculateRentalTaxAndPrice;

        // setters

        var setAgreement = function(e) {
            state.agreementType = Number(e.target.value);
            if (state.agreementType === 1 || state.agreementType === 2) {
                recalculateAndRenderRentalValues();
            } else {
                recalculateValuesAndRender();
            }
        };

        var setLoanAmortTerm = function(optionKey) {
            return function(loanTerm, amortTerm) {
                state[optionKey].loanTerm = parseFloat(loanTerm);
                state[optionKey].amortTerm = parseFloat(amortTerm);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setLoanTerm = function (optionKey) {
            return function (e) {
                state[optionKey].LoanTerm = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setAmortTerm = function (optionKey) {
            return function (e) {
                state[optionKey].AmortizationTerm = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setDeferralPeriod = function(optionKey) {
            return function(e) {
                state[optionKey].DeferralPeriod = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setCustomerRate = function(optionKey) {
            return function(e) {
                state[optionKey].CustomerRate = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setYourCost = function(optionKey) {
            return function(e) {
                state[optionKey].yourCost = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setAdminFee = function(optionKey) {
            return function(e) {
                state[optionKey].AdminFee = parseFloat(e.target.value);
                recalculateValuesAndRender([{ name: optionKey }]);
            };
        };

        var setDownPayment = function(e) {
            state.downPayment = parseFloat(e.target.value);
            recalculateValuesAndRender();
        };

        var setRentalMPayment = function(e) {
            state.rentalMPayment = parseFloat(e.target.value);
            recalculateRentalTaxAndPrice();
        };

        var updateCost = function(id) {
            return function(e) {
                if (!state.equipments.hasOwnProperty(id)) {
                    return;
                }

                state.equipments[id].cost = parseFloat(e.target.value);
                if (state.agreementType === 1 || state.agreementType === 2) {
                    recalculateAndRenderRentalValues();
                } else {
                    recalculateValuesAndRender();
                }
            };
        };

        var removeEquipment = function(id) {
            return function() {
                if (!state.equipments.hasOwnProperty(id)) {
                    return;
                }

                state.equipments[id].template.remove();
                delete state.equipments[id];
                if (state.agreementType === 1 || state.agreementType === 2) {
                    recalculateAndRenderRentalValues();
                } else {
                    recalculateValuesAndRender();
                }
            };
        };

        var submitForm = function (event) {
            var agreementType = $("#typeOfAgreementSelect").find(":selected").val();
            if (agreementType === "0") {
                var rateCard = $('.checked');
                if (rateCard.length === 0) {
                    event.preventDefault();
                } else {
                    var monthPayment = Globalize.parseNumber($("#totalPrice").text());

                    if (isNaN(monthPayment) || (monthPayment == 0)) {
                        event.preventDefault();
                        $('#new-equipment-validation-message')
                            .text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                    } else {
                        var option = rateCard.find('#hidden-option').text();
                        if (option === 'Custom') {

                            var customSlicedTotalMPayment = $('#' + option + 'TMPayments').text().substring(1);
                            $('#AmortizationTerm').val(state[option].AmortizationTerm);
                            $('#LoanTerm').val(state[option].LoanTerm);
                            $('#CustomerRate').val(state[option].CustomerRate);
                            $('#AdminFee').val(state[option].AdminFee);
                            $('#total-monthly-payment').val(customSlicedTotalMPayment);
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
                                var cardType = $.grep(rateCards, function (c) { return c.name === option; })[0].id;

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

        var resetFormValidator = function (formId) {
            $(formId).removeData('validator');
            $(formId).removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(formId);
        }

        var addEquipment = function () {
            var id = $('div#new-equipments').find('[id^=new-equipment-]').length;
            var newId = id.toString();
            state.equipments[newId] = {
                id: newId,
                type: '',
                description: '',
                cost: '',
            };

            var newTemplate = equipmentTemplateFactory($('<div></div>'), { id: id });

            state.equipments[newId].template = newTemplate;

            // equipment handlers
            newTemplate.find('.equipment-cost').on('change', updateCost(newId));
            newTemplate.find('.glyphicon-remove').on('click', removeEquipment(newId));

            $('#new-equipments').append(newTemplate);

            resetFormValidator("#equipment-form");
        };

        // submit
        $('#equipment-form').submit(submitForm);

        // handlers
        $('#addEquipment').on('click', addEquipment);
        $('#downPayment').on('change', setDownPayment);
        $('#typeOfAgreementSelect').on('change', setAgreement);
        $('#total-monthly-payment').on('change', setRentalMPayment);
        $('.btn-select-card').on('click', function () {
            recalculateValuesAndRender();
        });

        // custom option
        $('#CustomLoanTerm').on('change', setLoanTerm('Custom'));
        $('#CustomAmortTerm').on('change', setAmortTerm('Custom'));
        $('#CustomDeferralPeriod').on('change', setDeferralPeriod('Custom'));
        $('#CustomCustomerRate').on('change', setCustomerRate('Custom'));
        $('#CustomYourCost').on('change', setYourCost('Custom'));
        $('#CustomAdminFee').on('change', setAdminFee('Custom'));

        // deferral
        $('#deferralLATerm').on('change', function(e) {
            var loanAmort = e.target.value.split('/');
            setLoanAmortTerm('deferral')(loanAmort[0], loanAmort[1]);
        });
        
        var equipments = $('div#new-equipments').find('[id^=new-equipment-]').length;

        var initEquipment = function (i) {
            var cost = parseFloat($('#NewEquipment_' + i + '__Cost').val());
            if (state.equipments[i] === undefined) {
                state.equipments[i] = { cost: cost }
            } else {
                state.equipments[i].cost = cost;
            }

            $('#new-equipment-' + i).find('.equipment-cost').on('change', updateCost(i));
        }

        for (var i = 0; i <= equipments; i++) {
            // attatch handler to equipments
            initEquipment(i);
            recalculateValuesAndRender();
        }
    });