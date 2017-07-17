module.exports('calculator-value-setters', function (require) {
    var state = require('calculator-state');

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
        return function (loanTerm, amortTerm) {
            state[optionKey].LoanTerm = parseFloat(loanTerm);
            state[optionKey].AmortizationTerm = parseFloat(amortTerm);
            callback([{ name: optionKey }]);
        };
    };

    var setDeferralPeriod = function (optionKey, callback) {
        return function (e) {
            state[optionKey].DeferralPeriod = parseFloat(e.target.value);
            callback([{ name: optionKey }]);
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

    var setAdminFee = function (optionKey, callback) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            callback([{ name: optionKey }]);
        };
    };

    var setDownPayment = function (optionKey, callback) {
        state[optionKey].downPayment = parseFloat(e.target.value);
        callback();
    };

    var setRentalMPayment = function (e) {
        state.rentalMPayment = parseFloat(e.target.value);
        recalculateRentalTaxAndPrice();
    };

    return {
        notNaN: notNaN,
        equipmentSum: equipmentSum,
        setLoanAmortTerm: setLoanAmortTerm,
        setDeferralPeriod: setDeferralPeriod,
        setCustomerRate: setCustomerRate,
        setYourCost: setYourCost,
        setAdminFee: setAdminFee,
        setDownPayment: setDownPayment
    }
});