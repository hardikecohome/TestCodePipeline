module.exports('calculator-state', function () {
    var state = {
        option1: {
            equipments: {},
            plan: 0,
            downPayment: 0
        },
        tax: 0,
        equipmentNextIndex: 0
    };

    var constants = {
        rateCards: [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }, { id: 3, name: 'Custom' }],
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm'],
        customInputsToShow: ['-customYCostVal', '-customAFee', '-customCRate', '-deferralDropdownWrapper', '-customLoanAmortWrapper'],
        inputsToHide: ['-cRate', '-yCostVal', '-aFee'],
        emptyRateCard: {
            Id: '',
            LoanTerm: '',
            AmortizationTerm: '',
            DeferralPeriod: 3,
            CustomerRate: '',
            yourCost: '',
            DealerCost: 0,
            AdminFee: 0
        },
        minimumLoanValue: 1000,
        amortizationValueToDisable : 180,
        totalAmountFinancedFor180amortTerm: 5000,
        maxRateCardLoanValue: 50000
    };

    window.state = state;

    return {
        state: state,
        constants: constants
    }
});