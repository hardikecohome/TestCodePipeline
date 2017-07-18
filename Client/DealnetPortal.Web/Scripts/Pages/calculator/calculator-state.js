module.exports('calculator-state', function () {
    var state = {
        'option1': {},
        'tax': 10
    };
    var constants = {
        rateCards: [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }]
    };

    return {
        state: state,
        constants: constants
    }
});