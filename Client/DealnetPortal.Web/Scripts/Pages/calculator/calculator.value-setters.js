module.exports('calculator-value-setters', function (require) {
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    var notNaN = function (num) {
        return !isNaN(num);
    };

    var idToValue = require('idToValue');

    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) {
                return parseFloat(equipment.cost);
            })
            .filter(notNaN)
            .reduce(function (sum, cost) {
                return sum + cost;
            }, 0);
    };

    var setReductionCost = function(optionKey, callback) {
        return function(e) {
            var selectedOption = e.target.options[e.target.selectedIndex];
            var intRate = +selectedOption.getAttribute('intrate');
            var custRate = +selectedOption.getAttribute('custRate');
            state[optionKey].InterestRateReduction = intRate;
            state[optionKey].CustomerReduction = custRate;
            callback([optionKey]);
        }
    }

    var setLoanAmortTerm = function (optionKey, callback) {
        return function (e) {
            state[optionKey].InterestRateReduction = 0;
            state[optionKey].CustomerReduction = 0;
            state[optionKey].ReductionId = null;
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
            $(e.target).valid();
            state[optionKey].CustomerRate = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setYourCost = function (optionKey, callback) {
        return function (e) {
            $(e.target).valid();
            state[optionKey].DealerCost = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setAdminFee = function (optionKey, callback) {
        return function (e) {
            $(e.target).valid();
            state[optionKey].AdminFee = Number(e.target.value);
            callback([optionKey]);
        };
    };

    var setDownPayment = function (optionKey, callback) {
        return function (e) {
            $(e.target).valid();
            state[optionKey].downPayment = e.target.value !== '' ? parseFloat(e.target.value) : 0;
            callback([optionKey]);
        }
    };

    var setAdminFeeIsCovered = function(optionKey, val, callback) {
        state[optionKey].includeAdminFee = val;
        callback([optionKey]);
    }

    var setProgram = function(optionKey, callback) {
        return function(e) {
            state[optionKey].Program = e.target.value;
            callback([optionKey]);
        }
    }

    var setRateCardPlan = function (optionKey, callback) {
        return function (e) {
            var planType = $.grep(constants.rateCards, function (c) {
                return c.name === e.target.value;
            })[0].name;
            state[optionKey].plan = planType;

            var dropdownParentDiv = $('#' + optionKey + '-deferralDropdownWrapper');
            //var e = document.getElementById(optionKey + '-amortDropdown');
            //e.selectedIndex = -1;
            if (constants.reductionCards.indexOf(planType) !== -1) {
                $('#' + optionKey + '-reductionWrapper').removeClass('hidden');
            } else {
                $('#' + optionKey + '-reductionWrapper').addClass('hidden');
            }

            if (planType === 'Deferral') {
                if (dropdownParentDiv.is(':hidden')) {
                    $('#' + optionKey + '-deferral').addClass('hidden');
                    dropdownParentDiv.removeClass('hidden');
                    $('#' + optionKey + '-deferralDropdown').change();
                }

                $('#' + optionKey + '-deferralDropdown').empty();
                $('#deferralDropdownForDeferralRc option').clone().appendTo('#' + optionKey + '-deferralDropdown');

                $('#' + optionKey + '-amortDropdown').closest('.row').removeClass('hidden');

                if (state.programsAvailable) {
                    $('#' + optionKey + '-programDropdown').closest('.row').removeClass('hidden');
                }

                $.grep(constants.inputsToHide, function (field) {
                    $('#' + optionKey + field).removeClass('hidden');
                });

                dropdownParentDiv.removeClass('hidden').attr('disabled', false);
            } else {
                $('#' + optionKey + '-deferral').removeClass('hidden');

                if (dropdownParentDiv.is(':visible')) {
                    dropdownParentDiv.addClass('hidden');
                }

                if (state.programsAvailable) {
                    $('#' + optionKey + '-programDropdown').closest('.row').removeClass('hidden');
                }

                $('#' + optionKey + '-amortDropdown').closest('.row').removeClass('hidden');

                $.grep(constants.inputsToHide, function (field) {
                    $('#' + optionKey + field).removeClass('hidden');
                });
            }

            callback([optionKey], true);
        }
    }

    var setEquipmentCost = function (optionKey, callback) {
        return function (e) {
            $(e.target).valid();
            var mvcId = e.target.id;
            var id = mvcId.split('__Cost')[0].substr(mvcId.split('__Cost')[0].lastIndexOf('_') + 1);

            state[optionKey].equipments[id].cost = isNaN(parseFloat(e.target.value)) ? '' : parseFloat(e.target.value);
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

    var setEquipmentDescription = function (optionKey) {
        return function (e) {
            $(e.target).valid();
            var mvcId = e.target.id;
            var id = mvcId.split('__Description')[0].substr(mvcId.split('__Description')[0].lastIndexOf('_') + 1);
            state[optionKey].equipments[id].description = e.target.value;
        }
    }

    var setNewEquipment = function (optionKey, callback) {
        var nextIndex = state[optionKey].equipmentNextIndex;
        state[optionKey].equipments[nextIndex] = {
            id: nextIndex.toString(),
            cost: '',
            description: '',
            type: state.defaultEquipment
        };
        state[optionKey].equipmentNextIndex++;

        callback([optionKey]);
    }

    var removeEquipment = function (optionKey, id, callback) {
        $('#' + optionKey + '-equipment-' + id).remove();
        state[optionKey].equipmentNextIndex--;

        callback([optionKey]);
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
        setProgram: setProgram,
        setReductionCost: setReductionCost,
        setAdminFeeIsCovered: setAdminFeeIsCovered
    }
});