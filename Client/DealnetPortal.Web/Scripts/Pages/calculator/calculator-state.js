module.exports('calculator-state', function () {
    var state = {
        option1: {
            equipments: {},
            plan: 0,
            downPayment: 0
        },
        tax: 10,
        equipmentNextIndex: 0
    };

    var constants = {
        rateCards: [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }],
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm']
    };

    window.state = state;

    return {
        state: state,
        constants: constants
    }
});