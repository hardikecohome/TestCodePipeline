﻿module.exports('rate-cards-init', function (require) {
    var state = require('state').state;
    var constants = require('state').constants;
    var rateCardBlock = require('rate-cards-ui');
    var rateCardsCaclulationEngine = require('rateCards.index');

    /**
     * Initialize view and store rate cards in storage.
     * @param {number} id - contract id
     * @param {Array<string>} cards - array of rate cards which specifed to dealer 
     * @param {boolean} onlyCustomRateCard - if no available rate cards, show only custom rate card 
     * @returns {void} 
     */
    var init = function(id, cards, onlyCustomRateCard) {
        state.contractId = id;
        // check if we have any prefilled values in database
        // related to this contract, if yes contract is not new
        state.isNewContract = $('#IsNewContract').val().toLowerCase() === 'true';
        state.selectedCardId = $('#SelectedRateCardId').val() !== "" ? +$('#SelectedRateCardId').val() : null;
        state.onlyCustomRateCard = onlyCustomRateCard;
        var isValidRateCard = $('#RateCardValid').val().toLocaleLowerCase() === 'true';

        if (!isValidRateCard && !state.isNewContract) {
            $('#expired-rate-card-warning').removeClass('hidden');
        }

        if (state.isNewContract && state.selectedCardId === null) {
            $('#submit').addClass('disabled');
            $('#submit').parent().popover();
        } else {
            $('#submit').parent().popover('destroy');
        }

        if (state.onlyCustomRateCard) {
            if (state.selectedCardId !== null) {
                renderRateCardOption('Custom');
            }
        } else {
            rateCardsCaclulationEngine.init(cards);

            if (state.selectedCardId !== null) {
                constants.rateCards.forEach(function (option) {
                    var filtred = $.grep(cards, function (card) { return card.CardType === option.id; });
                    renderRateCardOption(option.name, filtred);
                });
            }
        }
    };

    /**
     * Hide/Show block with rate cards and highlight selected rate card on initialization
     * @param {string} option - rate card option [NoInterest, Fixed, Deferral, Custom]
     * @param {Array<string>} items - list of rate cards for the option
     * @returns {void} 
     */
    function renderRateCardOption (option, items) {
        rateCardBlock.toggle(state.isNewContract);
        if (option !== 'Custom' && state.selectedCardId !== 0) {
            setSelectedRateCard(option, items);

            if (option === 'Deferral') {
                var deferralPeriod = $.grep(constants.customDeferralPeriods,
                    function (period) { return period.name === $('#LoanDeferralType').val(); })[0];

                if (deferralPeriod !== null && deferralPeriod !== undefined && deferralPeriod.val !== 0) {
                    $('#DeferralPeriodDropdown').val(deferralPeriod.val.toString());
                }
            }
        }

        if (option === 'Custom' && state.selectedCardId === 0) {
            setSelectedCustomRateCard();
            rateCardBlock.highlightCardBySelector('#CustomLoanTerm');
        }
    }

    /**
     * Highlight selected rate on initialization 
    * @param {string} option - rate card option [NoInterest, Fixed, Deferral, Custom]
     * @param {Array<string>} items - list of rate cards for the option
     * @returns {void} 
     */
    function setSelectedRateCard (option, items) {
        var selectedCard = $.grep(items, function (card) { return card.Id === Number(state.selectedCardId); })[0];
        if (selectedCard !== null && selectedCard !== undefined) {
            state[option] = selectedCard;
            state[option].yourCost = '';

            rateCardBlock.togglePromoLabel(option);

            //just find parent div
            rateCardBlock.highlightCardBySelector('#' + option + 'AFee');

            $('#' + option + '-amortDropdown').val(state[option].AmortizationTerm);
            $('#' + option + 'AFee').text(formatCurrency(state[option].AdminFee));
            $('#' + option + 'CRate').text(state[option].CustomerRate + ' %');
            $('#' + option + 'YCostVal').text(state[option].DealerCost + ' %');
        }
    }

    /**
     * populate custom rate cards with values
     * @returns {void} 
     */
    function setSelectedCustomRateCard () {
        var deferralPeriod = $.grep(constants.customDeferralPeriods, function (period) { return period.name === $('#LoanDeferralType').val(); })[0];

        state['Custom'].LoanTerm = Number($('#LoanTerm').val());
        state['Custom'].AmortizationTerm = Number($('#AmortizationTerm').val());
        state['Custom'].DeferralPeriod = deferralPeriod === undefined ? 0 : deferralPeriod.val;
        state['Custom'].CustomerRate = Number($('#CustomerRate').val());
        state['Custom'].AdminFee = Number($('#AdminFee').val());
        state['Custom'].DealerCost = Number($('#DealerCost').val());

        $('#CustomLoanTerm').val(state['Custom'].LoanTerm);
        $('#CustomAmortTerm').val(state['Custom'].AmortizationTerm);
        $('#CustomDeferralPeriod').val(state['Custom'].DeferralPeriod);
        $('#CustomCRate').val(state['Custom'].CustomerRate);
        $('#CustomAFee').val(state['Custom'].AdminFee);
        $('#CustomYCostVal').val(state['Custom'].DealerCost);
    }

    return {
        init: init
    };
});