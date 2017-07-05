module.exports('state', function() {
    var state = {
        agreementType: 0,
        equipments: {},
        tax: taxRate,
        downPayment: 0,
        rentalMPayment: 0,
        Custom: {
            LoanTerm: '',
            AmortizationTerm: '',
            DeferralPeriod: 0,
            CustomerRate: '',
            yourCost: '',
            DealerCost: 0,
            AdminFee: 0
        },
        contractId: 0,
        selectedCardId: null,
        isInitialized: false,
        isNewContract: true
    };

    var constants = {
        rateCards: [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }, { id: 3, name: 'Custom' }],
        customDeferralPeriods: [{ val: 0, name: 'NoDeferral' }, { val: 3, name: 'ThreeMonth' }, { val: 6, name: 'SixMonth' }, { val: 9, name: 'NineMonth' }, { val: 12, name: 'TwelveMonth' }],
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm'],
        minimumLoanValue: 1000
    }

    window.state = state;

    return {
        state: state,
        constants: constants
    }
})