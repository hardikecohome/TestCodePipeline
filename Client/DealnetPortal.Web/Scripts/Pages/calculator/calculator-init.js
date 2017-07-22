module.exports('calculator-init', function (require) {

    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    /**
     * Initialize rate cards, page state, available plans
     * @param {Array<string>} plans - list of available rate card plans [ FixedRate, NoInterest, Deferral] 
     * @param {Array<object>} rateCards - list of rate cards for current user
     * @returns {} 
     */
    var init = function initPage(plans, rateCards) {
        state.cards = rateCards;
        var planIds = Object.keys(constants.rateCards).map(function(p) {
            return constants.rateCards[p].id;
        });

        state.amortLoanPeriods = {};
        planIds.forEach(function (p) {
            state.amortLoanPeriods[p] = [];

            var filtred = state.cards.filter(function(f) {
                return f.CardType === p;
            });

            $.grep(filtred, function(a) {
                var key = a.LoanTerm + '/' + a.AmortizationTerm;

                if (state.amortLoanPeriods[p].indexOf(key) === -1) {
                    state.amortLoanPeriods[p].push(key);
                }
            });
        });
    }

    return { init: init };
});