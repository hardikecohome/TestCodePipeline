module.exports('calculator-init', function (require) {

    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    /**
     * Initialize rate cards, page state, available plans
     * @param {Array<string>} plans - list of available rate card plans [ FixedRate, NoInterest, Deferral] 
     * @param {Array<object>} rateCards - list of rate cards for current user
     * @returns {} 
     */
    var init = function initPage(plans, rateCards, taxes) {
        state.cards = rateCards;
        state.taxes = taxes;

        $('#option1-taxDescription').text(state.description);

        var planIds = Object.keys(constants.rateCards).map(function(p) {
            return constants.rateCards[p].id;
        });

        state.defaultEquipment = $('#Equipment_NewEquipment_0__Type').val();
        state.amortLoanPeriods = {};

        planIds.forEach(function (plan) {
            state.amortLoanPeriods[plan] = [];

            var filtred = state.cards.filter(function(f) {
                return f.CardType === plan;
            });

            $.grep(filtred, function(a) {
                var key = a.LoanTerm + '/' + a.AmortizationTerm;

                if (state.amortLoanPeriods[plan].indexOf(key) === -1) {
                    state.amortLoanPeriods[plan].push(key);
                }
            });
        });
    }

    return { init: init };
});