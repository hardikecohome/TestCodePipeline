module.exports('calculator-value-setters', function (require) {
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
            callback([ optionKey ]);
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
            callback([{ name: optionKey }]);
        };
    };

    var setYourCost = function (optionKey, callback) {
        return function (e) {
            state[optionKey].yourCost = parseFloat(e.target.value);
            callback([{ name: optionKey }]);
        };
    };

    var setAdminFee = function(optionKey, callback) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            callback([{ name: optionKey }]);
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

            setDropdownValues(optionKey, planType);

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

    var setRentalMPayment = function (e) {
        state.rentalMPayment = parseFloat(e.target.value);
        recalculateRentalTaxAndPrice();
    };

    function setDropdownValues(optionKey, planType) {
        var options = $('#' + optionKey + '-amortDropdown');
        options.empty();
        var dropdownValues = state.amortLoanPeriods[planType];
        
        dropdownValues.forEach(function (item) {
            var optionTemplate = $("<option />").val(item).text(item);

            options.append(optionTemplate);
        });
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
        setNewEquipment: setNewEquipment,
        removeEquipment: removeEquipment,
        setTax: setTax
    }
});