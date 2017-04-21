module.exports('new-equipment',
    function(require) {
        var equipmentTemplateFactory = require('equipment-template');

        var tax = require('financial-functions').tax;
        var totalObligation = require('financial-functions').totalObligation;
        var totalPrice = require('financial-functions').totalPrice;
        var totalAmountFinanced = require('financial-functions').totalAmountFinanced;
        var monthlyPayment = require('financial-functions').monthlyPayment;
        var totalMonthlyPayments = require('financial-functions').totalMonthlyPayments;
        var residualBalance = require('financial-functions').residualBalance;
        var totalBorrowingCost = require('financial-functions').totalBorrowingCost;
        var yourCost = require('financial-functions').yourCost;

        var state = {
            agreementType: 0,
            equipments: {
                '0': {
                    type: '',
                    description: '',
                    cost: 0,
                    template: '',
                },
            },
            tax: 12,
            downPayment: 0,
            rentalMPayment: 0,
            fixedRate: {
                loanTerm: 36,
                amortTerm: 36,
                deferralPeriod: '',
                customerRate: 5,
                yourCost: 10,
                adminFee: 100,
            },
            noInterest: {
                loanTerm: 36,
                amortTerm: 36,
                deferralPeriod: '',
                customerRate: 0,
                yourCost: 5,
                adminFee: 200,
            },
            deferral: {
                loanTerm: 36,
                amortTerm: 36,
                deferralPeriod: '',
                customerRate: 10,
                yourCost: 20,
                adminFee: 0,
            },
            custom: {
                loanTerm: '',
                amortTerm: '',
                deferralPeriod: '',
                customerRate: '',
                yourCost: '',
                adminFee: '',
            },
        };

        var notNaN = function (num) { return !isNaN(num); };
        var idToValue = function (obj) {
            return function(id) {
                return obj.hasOwnProperty(id) ? obj[id] : '';
            };
        };

        // render static fields
        var renderStaticFields = function(option, data) {
            $('#' + option + 'CRate').text(data.customerRate + ' %');
            $('#' + option + 'YCostVal').text(data.yourCost + ' %');
            $('#' + option + 'AFee').text(formatCurrency(data.adminFee));
        };

        ['fixedRate', 'noInterest', 'deferral'].forEach(function(option) {
            renderStaticFields(option, state[option]);
        });

        var numberFields = ['equipmentSum', 'loanTerm', 'amortTerm', 'customerRate', 'adminFee'];
        var notCero = ['equipmentSum', 'loanTerm', 'amortTerm'];

        var renderTotalPrice = function(data) {
            var notNan = !Object.keys(data).map(idToValue(data)).some(function(val) { return isNaN(val); });
            if (notNan) {
                $('#totalEquipmentPrice').text(formatCurrency(data.equipmentSum));
                $('#tax').text(formatCurrency(data.tax));
                $('#totalPrice').text(formatCurrency(data.totalPrice));
            } else {
                $('#totalEquipmentPrice').text('-');
                $('#tax').text('-');
                $('#totalPrice').text('-');
            }
        };

        var renderOption = function(option, data) {
            var notNan = !Object.keys(data).map(idToValue(data)).some(function(val) { return isNaN(val); });
            var validateNumber = numberFields.every(function(field) {
                return typeof data[field] === 'number';
            });

            var validateNotEmpty = notCero.every(function(field) {
                return data[field] !== 0;
            });

            if (notNan && validateNumber && validateNotEmpty) {
                $('#' + option  + 'MPayment').text(formatCurrency(data.monthlyPayment));
                $('#' + option  + 'CBorrowing').text(formatCurrency(data.costOfBorrowing));
                $('#' + option  + 'TAFinanced').text(formatCurrency(data.totalAmountFinanced));
                $('#' + option  + 'TMPayments').text(formatCurrency(data.totalMonthlyPayments));
                $('#' + option  + 'RBalance').text(formatCurrency(data.residualBalance));
                $('#' + option  + 'TObligation').text(formatCurrency(data.totalObligation));
                $('#' + option  + 'YCost').text(formatCurrency(data.yourCost));
            } else {
                $('#' + option  + 'MPayment').text('-');
                $('#' + option  + 'CBorrowing').text('-');
                $('#' + option  + 'TAFinanced').text('-');
                $('#' + option  + 'TMPayments').text('-');
                $('#' + option  + 'RBalance').text('-');
                $('#' + option  + 'TObligation').text('-');
                $('#' + option  + 'YCost').text('-');
            }
        };

        var equipmentSum = function (equipments) {
            return Object.keys(equipments)
                .map(idToValue(equipments))
                .map(function(equipment) { return parseFloat(equipment.cost); })
                .filter(notNaN)
                .reduce(function(sum, cost) { return sum + cost; }, 0);
        };

        var allOptions = ['custom', 'deferral', 'fixedRate', 'noInterest'];
        var recalculateValuesAndRender = function(options) {
            var optionsToCompute = options || allOptions;

            var eSum = equipmentSum(state.equipments);

            renderTotalPrice({
                equipmentSum: eSum !== 0 ? eSum : '-',
                tax: eSum !== 0 ? tax({ equipmentSum: eSum, tax: state.tax }) : '-',
                totalPrice: eSum !== 0 ? totalPrice({ equipmentSum: eSum, tax: state.tax }) : '-'
            });

            optionsToCompute.forEach(function(option) {
                var data = $.extend({}, idToValue(state)(option),
                {
                    equipmentSum: eSum,
                    downPayment: state.downPayment,
                    tax: state.tax,
                });

                renderOption(option, $.extend({}, data, {
                    monthlyPayment: monthlyPayment(data),
                    costOfBorrowing: totalBorrowingCost(data),
                    totalAmountFinanced: totalAmountFinanced(data),
                    totalMonthlyPayments: totalMonthlyPayments(data),
                    residualBalance: residualBalance(data),
                    totalObligation: totalObligation(data),
                    yourCost: yourCost(data),
                }));
            });
        };

        var recalculateAndRenderRentalValues = function() {
            var eSum = equipmentSum(state.equipments);

            var data = {
                tax: state.tax,
                equipmentSum: eSum,
            };

            var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
            if (notNan && data.equipmentSum !== 0) {
                $('#rentalMPayment').val(eSum);
                $('#rentalTax').text(tax(data));
                $('#rentalTMPayment').text(totalPrice(data));
            } else {
                $('#rentalMPayment').val('');
                $('#rentalTax').text('-');
                $('#rentalTMPayment').text('-');
            }
        };

        var recalculateRentalTaxAndPrice = function() {
            var data = {
                tax: state.tax,
                equipmentSum: state.rentalMPayment,
            };

            var notNan = !Object.keys(data).map(idToValue(data)).some(function (val) { return isNaN(val); });
            if (notNan && data.equipmentSum !== 0) {
                $('#rentalTax').text(tax(data));
                $('#rentalTMPayment').text(totalPrice(data));
            } else {
                $('#rentalTax').text('-');
                $('#rentalTMPayment').text('-');
            }
        };

        // setters

        var setAgreement = function(e) {
            state.agreementType = Number(e.target.value);
            if (state.agreementType === 1 || state.agreementType === 2) {
                recalculateAndRenderRentalValues();
            } else {
                recalculateValuesAndRender();
            }
        };

        var setLoanTerm = function(optionKey) {
            return function(e) {
                state[optionKey].loanTerm = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setAmortTerm = function(optionKey) {
            return function(e) {
                state[optionKey].amortTerm = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setLoanAmortTerm = function(optionKey) {
            return function(loanTerm, amortTerm) {
                state[optionKey].loanTerm = parseFloat(loanTerm);
                state[optionKey].amortTerm = parseFloat(amortTerm);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setDeferralPeriod = function(optionKey) {
            return function(e) {
                state[optionKey].deferralPeriod = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setCustomerRate = function(optionKey) {
            return function(e) {
                state[optionKey].customerRate = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setYourCost = function(optionKey) {
            return function(e) {
                state[optionKey].yourCost = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
            };
        };

        var setAdminFee = function(optionKey) {
            return function(e) {
                state[optionKey].adminFee = parseFloat(e.target.value);
                recalculateValuesAndRender([optionKey]);
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

        var submitForm = function(event) {
            var agreementType = $("#agreement-type").find(":selected").val();
            if (agreementType === "0") {
                //isCalculationValid = false;
                //recalculateTotalCashPrice();
                if (!isCalculationValid) {
                    event.preventDefault();
                    $('#new-equipment-validation-message').text(translations['TotalMonthlyPaymentMustBeGreaterZero']);
                }
            } else {
                var monthPayment = Globalize.parseNumber($("#total-monthly-payment").val());
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


        var id = $('div#new-equipments').find('[id^=new-equipment-]').length;
        var addEquipment = function() {
            var newId = id.toString();
            state.equipments[newId] = {
                id: newId,
                type: '',
                description: '',
                cost: '',
            };

            var index = Object.keys(state.equipments).length;
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
        $('#customLoanTerm').on('change', setLoanTerm('custom'));
        $('#customAmortTerm').on('change', setAmortTerm('custom'));
        $('#customDeferralPeriod').on('change', setDeferralPeriod('custom'));
        $('#customCustomerRate').on('change', setCustomerRate('custom'));
        $('#customYourCost').on('change', setYourCost('custom'));
        $('#customAdminFee').on('change', setAdminFee('custom'));

        // deferral
        $('#deferralLATerm').on('change', function(e) {
            var loanAmort = e.target.value.split('/');
            setLoanAmortTerm('deferral')(loanAmort[0], loanAmort[1]);
        });

        // attatch handler to first equipment
        $('#new-equipment-0').find('.equipment-cost').on('change', updateCost(0));
    });