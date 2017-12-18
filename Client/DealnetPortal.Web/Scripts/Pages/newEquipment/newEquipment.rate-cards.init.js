module.exports('rate-cards-init', function (require) {
    var state = require('state').state;
    var constants = require('state').constants;
    var rateCardBlock = require('rate-cards-ui');
    var rateCardsCaclulationEngine = require('rateCards.index');
    var customRateCardBlock = require('custom-rate-card');

    var settings = {
        isNewContractId: '#IsNewContract',
        selectedRateCardId: '#SelectedRateCardId',
        rateCardValidId: '#RateCardValid',
        submitButtonId: '#submit',
        customRateCardName: 'Custom',
        deferralRateCardName: 'Deferral'
    }

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
        state.isNewContract = $(settings.isNewContractId).val().toLowerCase() === 'true';
        state.selectedCardId = $(settings.selectedRateCardId).val() !== "" ? +$(settings.selectedRateCardId).val() : null;
        state.onlyCustomRateCard = onlyCustomRateCard;
        var isValidRateCard = $(settings.rateCardValidId).val().toLocaleLowerCase() === 'true';

        if (!isValidRateCard && !state.isNewContract) {
            $('#expired-rate-card-warning').removeClass('hidden');
        }

        var $submitBtnSelector = $(settings.submitButtonId);
        if (state.isNewContract && state.selectedCardId === null) {
            $submitBtnSelector.addClass('disabled');
            $submitBtnSelector.parent().popover();
        } else {
            $submitBtnSelector.parent().popover('destroy');
        }

        if (state.onlyCustomRateCard) {
            if (state.selectedCardId !== null) {
                renderRateCardOption(settings.customRateCardName);
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
        if (option !== settings.customRateCardName && state.selectedCardId !== 0) {
            setSelectedRateCard(option, items);

            if (option === settings.deferralRateCardName) {
                var deferralPeriod = $.grep(constants.customDeferralPeriods,
                    function (period) { return period.name === $('#LoanDeferralType').val(); })[0];

                if (deferralPeriod !== null && deferralPeriod !== undefined && deferralPeriod.val !== 0) {
                    $('#DeferralPeriodDropdown').val(deferralPeriod.val.toString());
                }
            }
        }

        if (option === settings.customRateCardName && state.selectedCardId === 0) {
            customRateCardBlock.setSelectedCustomRateCard();
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

    return {
        init: init
    };
});