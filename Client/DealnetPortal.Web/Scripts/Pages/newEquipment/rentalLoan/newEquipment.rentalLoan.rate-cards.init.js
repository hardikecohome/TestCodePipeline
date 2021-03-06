﻿module.exports('rate-cards-init', function (require) {
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
        deferralRateCardName: 'Deferral',
        customSelectedId: '#custom-selected',
        rateReductionId: '#rateReductionId'
    }

    /**
     * Initialize view and store rate cards in storage.
     * @param {number} id - contract id
     * @param {Array<string>} cards - array of rate cards which specifed to dealer 
     * @param {boolean} onlyCustomRateCard - if no available rate cards, show only custom rate card 
     * @returns {void} 
     */
    var init = function (id, cards, rateCardReduction) {
        state.contractId = id;

        state.customSelected = $(settings.customSelectedId).val().toLowerCase() === 'true';
        state.selectedCardId = $(settings.selectedRateCardId).val() !== "" ? +$(settings.selectedRateCardId).val() : null;
        if (state.selectedCardId === 0) state.selectedCardId = null;
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

        rateCardsCaclulationEngine.init(cards, rateCardReduction);

        if (state.onlyCustomRateCard) {
            if (state.customSelected) {
                _renderRateCardOption(settings.customRateCardName);
            }
        } else {

            _setCustomRateCardAdminFee(cards);

            constants.rateCards.forEach(function (option) {
                var filtred = $.grep(cards, function (card) {
                    return card.CardType === option.id;
                });
                _renderRateCardOption(option.name, filtred, rateCardReduction);
            });

        }
    };

    /**
     * Hide/Show block with rate cards and highlight selected rate card on initialization
     * @param {string} option - rate card option [NoInterest, Fixed, Deferral, Custom]
     * @param {Array<string>} items - list of rate cards for the option
     * @returns {void} 
     */
    function _renderRateCardOption(option, items, rateCardReduction) {
        rateCardBlock.toggle(state.isNewContract);
        if (option !== settings.customRateCardName && !state.customSelected) {
            _setSelectedRateCard(option, items, rateCardReduction);

            if (option === settings.deferralRateCardName) {
                var deferralPeriod = $.grep(constants.customDeferralPeriods,
                    function (period) {
                        return period.name === $('#LoanDeferralType').val();
                    })[0];

                if (deferralPeriod !== null && deferralPeriod !== undefined && deferralPeriod.val !== 0) {
                    $('#DeferralPeriodDropdown').val(deferralPeriod.val.toString());
                }
            }
        }

        if (option === settings.customRateCardName && state.customSelected) {
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
    function _setSelectedRateCard(option, items, rateCardReduction) {
        var selectedCard = $.grep(items, function (card) {
            return card.Id === Number(state.selectedCardId);
        })[0];
        var reductionRate;
        var id = $(settings.rateReductionId).val();
        if (id) {
            reductionRate = rateCardReduction.filter(function (reduct) {
                return reduct.Id === Number(id)
            })[0];
        }
        if (selectedCard !== null && selectedCard !== undefined) {
            state[option] = selectedCard;
            if (reductionRate) {
                state[option].CustomerReduction = reductionRate.CustomerReduction;
                state[option].InterestRateReduction = reductionRate.InterestRateReduction;
                state[option].ReductionId = reductionRate.Id;
            }
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

    function _setCustomRateCardAdminFee(cards) {
        var customRcId = constants.rateCards.filter(function (card) {
            return card.name === settings.customRateCardName
        })[0].id;
        var customRateCards = cards.filter(function (card) {
            return card.CardType === customRcId
        });
        if (!customRateCards.length) return;

        $.grep(customRateCards, function (card) {
            state.customRateCardBoundaires[card.LoanValueFrom + '-' + card.LoanValueTo] = {
                adminFee: card.AdminFee
            };
        });
    }
    return {
        init: init
    };
});