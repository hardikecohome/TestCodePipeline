module.exports('calculator.init', function (require) {

    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;
    var calculatorOption = require('calculator.option');
    var calculatorJCourusel = require('calculator.jcarousel');
    var rateCardsCalculator = require('rateCards.index');

    /**
     * Initialize rate cards, page state, available plans
     * @param {Array<string>} plans - list of available rate card plans [ FixedRate, NoInterest, Deferral] 
     * @param {Array<object>} rateCards - list of rate cards for current user
     * @returns {} 
     */
    function _initRateCards(plans, rateCards, taxes, rateCardReductionTable) {
        state.cards = rateCards;
        state.taxes = taxes;
        rateCardsCalculator.init(rateCards, rateCardReductionTable);
        state.programsAvailable = $('#programs-available').val().toLowerCase() === 'true';
        state.isDisplayAdminFee = $('#isPassAdminFee').val().toLowerCase() === 'true';

        if (!state.programsAvailable) {
            $('#option1-programDropdown').closest('.row').addClass('hidden');
        }

        if (state.isDisplayAdminFee) {
            $('#option1-aFeeOptionsHolder').removeClass('hidden');
        }

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

    var init = function (plans, rateCards, taxes, rateCardReductionTable) {
        _initRateCards(plans, rateCards, taxes, rateCardReductionTable);
        _setCustomRateCardAdminFee(rateCards);
        calculatorJCourusel.init();
        calculatorOption.init();
    }

    function _setCustomRateCardAdminFee(cards) {
        var customRcId = constants.rateCards.filter(function(card) { return card.name === 'Custom' })[0].id;
        var customRateCards = cards.filter(function(card) { return card.CardType === customRcId });
        if (!customRateCards.length) return;

        $.grep(customRateCards, function(card) {
            state.customRateCardBoundaires[card.LoanValueFrom + '-' + card.LoanValueTo] = { adminFee: card.AdminFee };
        });
    }

    return { init: init };
});