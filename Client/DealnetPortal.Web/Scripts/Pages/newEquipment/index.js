module.exports('new-equipment',
    function(require) {
        var equipmentTemplateFactory = require('equipment-template');
        var state = require('rate-cards').state;

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
                            $('#AmortizationTerm').val($('#CustomAmortTerm').text());
                            $('#CustomerRate').val($('#CustomCustomerRate').text());
                            $('#AdminFee').val($('#CustomAdminFee').text());
                        } else {
                            $('#AmortizationTerm').val($('#' + option + 'AmortizationDropdown').val());

                            //remove percentage symbol
                            var slicedCustomerRate = $('#' + option + 'CRate').text().slice(0, -2);
                            var slicedAdminFee = $('#' + option + 'AFee').text().substring(1);

                            $('#CustomerRate').val(slicedCustomerRate);
                            $('#AdminFee').val(slicedAdminFee);
                        }
                    }
                }
            } else {
                var monthPayment = Globalize.parseNumber($("#totalPrice").text());
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
            //$("#customer-rate").rules("add", "required");
            //$("#amortization-term").rules("add", "required");
            //$("#requested-term").rules("add", "required");
            //$("#loan-term").rules("add", "required");
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
        $('#rentalMPayment').on('change', setRentalMPayment);

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

        // attatch handler to first equipment
        $('#new-equipment-0').find('.equipment-cost').on('change', updateCost(0));
    });