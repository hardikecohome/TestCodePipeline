module.exports('rate-cards-init', function (require) {
    var state = require('state').state;
    var constants = require('state').constants;
    var rateCardBlock = require('rate-cards-ui');
    var recalculateValuesAndRender = require('rate-cards').recalculateValuesAndRender;

    /**
     * Initialize view and store rate cards in storage.
     * @param {number} id - contract id
     * @param {Array<>} cards - array of rate cards which specifed to dealer 
     * @param {boolean} onlyCustomRateCard - if no available rate cards, show only custom rate card 
     * @returns {} 
     */
    var init = function (id, cards, onlyCustomRateCard) {
        state.contractId = id;
        // check if we have any prefilled values in database
        // related to this contract, if yes contract is not new
        state.isNewContract = $('#IsNewContract').val().toLowerCase() === 'true';
        state.selectedCardId = $('#SelectedRateCardId').val() !== "" ? +$('#SelectedRateCardId').val() : null;
        state.onlyCustomRateCard = onlyCustomRateCard;
        var isValidRateCard = $('#RateCardValid').val().toLocaleLowerCase() === 'true';

        if (!isValidRateCard) {
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
                renderSelectedRateCardUi('Custom');
            }
        } else {
            constants.rateCards.forEach(function (option) {
                if (sessionStorage.getItem(state.contractId + option.name) !== null) {
                    sessionStorage.removeItem(state.contractId + option.name);
                }

                var filtred = $.grep(cards, function (card) { return card.CardType === option.id; });

                _createDropdowns(filtred, option);

                sessionStorage.setItem(state.contractId + option.name, JSON.stringify(filtred));

                if (state.selectedCardId !== null) {
                    var items = $.parseJSON(sessionStorage.getItem(state.contractId + option.name));
                    renderSelectedRateCardUi(option.name, items);
                }
            });
        }

        recalculateValuesAndRender();
    }

    /**
     * Hide/Show block with rate cards and highlight selected rate card on initialization
     * @param {string} option - rate card option [NoInterest, Fixed, Deferral, Custom]
     * @param {Array<>} items - list of rate cards for the option
     * @returns {} 
     */
    function renderSelectedRateCardUi (option, items) {
        rateCardBlock.toggle(state.isNewContract);
        if (option !== 'Custom' && state.selectedCardId !== 0) {
            setSelectedRateCard(option, items);

            if (option === 'Deferral') {
                var deferralPeriod = $.grep(constants.customDeferralPeriods,
                    function (period) { return period.name === $('#LoanDeferralType').val() })[0];

                if (deferralPeriod != null && deferralPeriod.val !== 0) {
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
     * @param {Array<>} items - list of rate cards for the option
     * @returns {} 
     */
    function setSelectedRateCard (option, items) {
        var selectedCard = $.grep(items, function (card) { return card.Id === Number(state.selectedCardId); })[0];
        if (selectedCard !== null && selectedCard !== undefined) {
            state[option] = selectedCard;
            state[option].yourCost = '';

            rateCardBlock.togglePromoLabel(option);

            //just find parent div
            rateCardBlock.highlightCardBySelector('#' + option + 'AFee');

            $('#' + option + 'AmortizationDropdown').val(state[option].AmortizationTerm);
            $('#' + option + 'AFee').text(formatCurrency(state[option].AdminFee));
            $('#' + option + 'CRate').text(state[option].CustomerRate + ' %');
            $('#' + option + 'YCostVal').text(state[option].DealerCost + ' %');
        }
    }

    /**
     * populate custom rate cards with values
     * @returns {} 
     */
    function setSelectedCustomRateCard () {
        var deferralPeriod = $.grep(constants.customDeferralPeriods, function (period) { return period.name === $('#LoanDeferralType').val() })[0];

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

    function _createDropdowns(items, option) {
        state[option.name + '-dropdowns'] = {};

        $.each(items, function () {
            var key = this.LoanValueFrom + '-' + this.LoanValueTo;
            if (!state[option.name + '-dropdowns'].hasOwnProperty(key)) {
                state[option.name + '-dropdowns'][key] = [];
            }

            var dropdownValue = ~~this.LoanTerm + '/' + ~~this.AmortizationTerm;

            if (state[option.name + '-dropdowns'][key].indexOf(dropdownValue) === -1) {
                state[option.name + '-dropdowns'][key].push(dropdownValue);
            }
        });
    }

    return {
        init: init
    }
});