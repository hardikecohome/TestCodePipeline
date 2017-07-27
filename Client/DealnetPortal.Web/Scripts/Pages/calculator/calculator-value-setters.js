﻿module.exports('calculator-value-setters', function (require) {
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    var notNaN = function (num) { return !isNaN(num); };

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };
    
    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var setLoanAmortTerm = function (optionKey, callback) {
        return function (e) {
            callback([optionKey]);
        };
    };

    var setLoanTerm = function (optionKey, callback) {
        return function (e) {
            state[optionKey].LoanTerm = parseFloat(e.target.value);
            validateLoanAmortTerm(optionKey);
            callback([ optionKey ]);
        };
    };

    var setAmortTerm = function (optionKey, callback) {
        return function (e) {
            state[optionKey].AmortizationTerm = parseFloat(e.target.value);
            validateLoanAmortTerm(optionKey);
            callback([optionKey]);
        };
    };

    var setDeferralPeriod = function (optionKey, callback) {
        return function (e) {
            state[optionKey].DeferralPeriod = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setCustomerRate = function (optionKey, callback) {
        return function (e) {
            state[optionKey].CustomerRate = parseFloat(e.target.value);
            callback([ optionKey ]);
        };
    };

    var setYourCost = function (optionKey, callback) {
        return function (e) {
            state[optionKey].DealerCost = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setAdminFee = function(optionKey, callback) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            callback([optionKey]);
        };
    };

    var setDownPayment = function(optionKey, callback) {
        return function(e) {
            state[optionKey].downPayment = parseFloat(e.target.value);
            callback([optionKey]);
        }
    };

    var setRateCardPlan = function(optionKey, callback) {
        return function (e) {
            var planType = $.grep(constants.rateCards, function (c) { return c.name === e.target.value; })[0].id;
            state[optionKey].plan = planType;

            setAmortizationDropdownValues(optionKey, planType);

            var dropdownParentDiv = $('#' + optionKey + '-deferralDropdownWrapper');

            if (planType === 2 || planType === 3) {
                if (dropdownParentDiv.is(':hidden')) {
                    $('#' + optionKey + '-deferral').addClass('hidden');
                    dropdownParentDiv.removeClass('hidden');

                    $('#' + optionKey + '-deferralDropdown').change();
                }

                if (planType === 3) {
                    $('#' + optionKey + '-amortDropdown').closest('.row').addClass('hidden');
                    $.grep(constants.inputsToHide, function(field) {
                        $('#' + optionKey + field).addClass('hidden');
                    });

                    $.grep(constants.customInputsToShow, function(field) {
                        $('#' + optionKey + field).removeClass('hidden').attr('disabled', false);
                    });
                }

            } else {
                $('#' + optionKey + '-deferral').removeClass('hidden');
                $('#' + optionKey + '-downPayment').val('');
                $('#' + optionKey + '-customLoanTerm').val('');
                $('#' + optionKey + '-customAmortTerm').val('');

                if (dropdownParentDiv.is(':visible')) {
                    dropdownParentDiv.addClass('hidden');
                }

                $('#' + optionKey + '-amortDropdown').closest('.row').removeClass('hidden');
                $.grep(constants.inputsToHide, function (field) {
                    $('#' + optionKey + field).removeClass('hidden');
                });
                
                $.grep(constants.customInputsToShow, function (field) {
                    $('#' + optionKey + field).addClass('hidden');

                    if (field !== '-deferralDropdown') {
                        $('#' + optionKey + field).attr('disabled', true).val('');
                    }
                });
            }

            callback([optionKey]);
        }
    }

    var setEquipmentCost = function(optionKey, callback) {
        return function (e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Cost')[0].substr(mvcId.split('__Cost')[0].lastIndexOf('_') + 1);

            state[optionKey].equipments[id].cost = +e.target.value;
            callback([optionKey]);
        }
    }

    var setEquipmentType = function (optionKey) {
        return function (e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Type')[0].substr(mvcId.split('__Type')[0].lastIndexOf('_') + 1);
            state[optionKey].equipments[id].type = e.target.value;
        }
    }

    var setEquipmentDescription = function(optionKey) {
        return function(e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Description')[0].substr(mvcId.split('__Description')[0].lastIndexOf('_') + 1);
            state[optionKey].equipments[id].description = e.target.value;
        }
    }

    var setNewEquipment = function(optionKey, callback) {
        state[optionKey].equipments[state.equipmentNextIndex] = { id: state.equipmentNextIndex.toString(), cost: '' };
        state.equipmentNextIndex++;

        callback([optionKey]);
    }

    var removeEquipment = function (optionKey, callback) {
        return function (e) {
            var id = e.target.id;
            id = +id.substr(id.lastIndexOf('-') + 1);
            delete state[optionKey].equipments[id];
            $('#equipment-' + id).remove();
            state.equipmentNextIndex--;

            callback([optionKey]);
        }
    }

    var setTax = function(callback) {
        return function(e) {
            var name = e.target.value;
            var filtered = state.taxes.filter(function(tax) {
                return tax.Province === name;
            });
            state.tax = filtered[0].Rate;
            state.description = filtered[0].Description;
            callback();
        }
    }

    function setAmortizationDropdownValues(optionKey, planType) {
        var options = $('#' + optionKey + '-amortDropdown');
        options.empty();
        var dropdownValues = state.amortLoanPeriods[planType];
        
        dropdownValues.forEach(function (item) {
            var optionTemplate = $("<option />").val(item).text(item);

            options.append(optionTemplate);
        });
    }

    function validateLoanAmortTerm(optionKey) {
        var amortTerm = state[optionKey].AmortizationTerm;
        var loanTerm = state[optionKey].LoanTerm;
        var error = $('#' + optionKey + '-amortLoanTermError');
        if (typeof amortTerm == 'number' && typeof loanTerm == 'number') {
            if (loanTerm > amortTerm) {
                if (error.is(':hidden')) {
                    error.show();
                    error.parent().find('input[type="text"]')
                        .addClass('input-validation-error');
                }
            } else {
                if (error.is(':visible')) {
                    error.hide();
                    error.parent().find('input[type="text"]')
                        .removeClass('input-validation-error');
                }
            }
        }
    }

    return {
        notNaN: notNaN,
        equipmentSum: equipmentSum,
        setLoanAmortTerm: setLoanAmortTerm,
        setDeferralPeriod: setDeferralPeriod,
        setCustomerRate: setCustomerRate,
        setYourCost: setYourCost,
        setAdminFee: setAdminFee,
        setDownPayment: setDownPayment,
        setRateCardPlan: setRateCardPlan,
        setEquipmentCost: setEquipmentCost,
        setEquipmentType: setEquipmentType,
        setEquipmentDescription: setEquipmentDescription,
        setNewEquipment: setNewEquipment,
        removeEquipment: removeEquipment,
        setTax: setTax,
        setLoanTerm: setLoanTerm,
        setAmortTerm: setAmortTerm
    }
});