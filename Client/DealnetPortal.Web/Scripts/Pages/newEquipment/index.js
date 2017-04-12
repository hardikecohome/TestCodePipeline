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

        var state = {
            equipments: {
                '0': {
                    type: '',
                    description: '',
                    cost: 0,
                    template: '',
                },
            },
            tax: 0,
            downPayment: 0,
            fixedRate: {
                loanTerm: 0,
                amortTerm: 0,
                deferralPeriod: 0,
                customerRate: 0,
                yourCost: 0,
                adminFee: 0,
            },
            noInterest: {
                loanTerm: 0,
                amortTerm: 0,
                deferralPeriod: 0,
                customerRate: 0,
                yourCost: 0,
                adminFee: 0,
            },
            defferal: {
                loanTerm: 0,
                amortTerm: 0,
                deferralPeriod: 0,
                customerRate: 0,
                yourCost: 0,
                adminFee: 0,
            },
            custom: {
                loanTerm: 0,
                amortTerm: 0,
                deferralPeriod: 0,
                customerRate: 0,
                yourCost: 0,
                adminFee: 0,
            },
        };

        var renderTotalPrice = function(data) {
            $('#totalEquipmentPrice').text(formatCurrency(data.equipmentSum));
            $('#tax').text(formatCurrency(data.tax));
            $('#totalPrice').text(formatCurrency(data.totalPrice));
        };

        var renderOption = function(option, data) {
            if (option === 'custom') {
                $('#customMPayment').text(formatCurrency(data.monthlyPayment));
                $('#customCBorrowing').text(formatCurrency(data.costOfBorrowing));
                $('#customTAFinanced').text(formatCurrency(data.totalAmountFinanced));
                $('#customTMPayments').text(formatCurrency(data.totalMonthlyPayments));
                $('#customRBalance').text(formatCurrency(data.residualBalance));
                $('#customTObligation').text(formatCurrency(data.totalObligation));
            }
        };

        var notNaN = function (num) { return !isNaN(num); };
        var idToValue = function (obj) {
            return function(id) {
                return obj.hasOwnProperty(id) ? obj[id] : '';
            };
        };

        var equipmentSum = function (equipments) {
            return Object.keys(equipments)
                .map(idToValue(equipments))
                .map(function(equipment) { return parseFloat(equipment.cost); })
                .filter(notNaN)
                .reduce(function(sum, cost) { return sum + cost; }, 0);
        };

        var allOptions = ['custom'];
        var recalculateValuesAndRender = function(options) {
            var optionsToCompute = options || allOptions;

            var eSum = equipmentSum(state.equipments);

            renderTotalPrice({
                equipmentSum: eSum,
                tax: tax({ equipmentSum: eSum, tax: state.tax }),
                totalPrice: totalPrice({ equipmentSum: eSum, tax: state.tax }),
            });

            optionsToCompute.forEach(function(option) {
                var data = $.extend({}, idToValue(state)(option),
                {
                    equipmentSum: eSum,
                    downPayment: state.downPayment,
                    tax: state.tax,
                });

                renderOption(option, {
                    monthlyPayment: monthlyPayment(data),
                    costOfBorrowing: totalBorrowingCost(data),
                    totalAmountFinanced: totalAmountFinanced(data),
                    totalMonthlyPayments: totalMonthlyPayments(data),
                    residualBalance: residualBalance(data),
                    totalObligation: totalObligation(data),
                });
            });
        };

        // setters

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

        var updateCost = function(id) {
            return function(e) {
                if (!state.equipments.hasOwnProperty(id)) {
                    return;
                }

                state.equipments[id].cost = parseFloat(e.target.value);
                recalculateValuesAndRender();
            };
        };


        var removeEquipment = function (id) {
            return function() {
                if (!state.equipments.hasOwnProperty(id)) {
                    return;
                }

                state.equipments[id].template.remove();
                delete state.equipments[id];
                recalculateValuesAndRender();
            };
        };

        var id = 1;
        var addEquipment = function() {
            var newId = id.toString();
            state.equipments[newId] = {
                id: newId,
                type: '',
                description: '',
                cost: '',
            };

            var index = Object.keys(state.equipments).length;
            var newTemplate = equipmentTemplateFactory($('<div></div>'), { index: index });

            state.equipments[newId].template = newTemplate;

            // equipment handlers
            newTemplate.find('.equipment-cost').on('change', updateCost(newId));
            newTemplate.find('.glyphicon-remove').on('click', removeEquipment(newId));

            $('#new-equipments').append(newTemplate);

            id++;
        };

        // handlers
        $('#addEquipment').on('click', addEquipment);
        $('#downPayment').on('change', setDownPayment);

        $('#customLoanTerm').on('change', setLoanTerm('custom'));
        $('#customAmortTerm').on('change', setAmortTerm('custom'));
        $('#customDeferralPeriod').on('change', setDeferralPeriod('custom'));
        $('#customCustomerRate').on('change', setCustomerRate('custom'));
        $('#customYourCost').on('change', setYourCost('custom'));
        $('#customAdminFee').on('change', setAdminFee('custom'));
        // attatch handler to first equipment
        $('#new-equipment-0').find('.equipment-cost').on('change', updateCost(0));

    });