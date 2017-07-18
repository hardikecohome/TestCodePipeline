module.exports('calculator-init', function (require) {

    var state = require('calculator-state').state;

    /**
     * Initialize rate cards, page state, available plans
     * @param {Array<string>} plans - list of available rate card plans [ FixedRate, NoInterest, Deferral] 
     * @param {Array<object>} rateCards - list of rate cards for current user
     * @returns {} 
     */
    var init = function initPage(plans, rateCards) {
        state.cards = rateCards;
    }

    return { init: init };
});