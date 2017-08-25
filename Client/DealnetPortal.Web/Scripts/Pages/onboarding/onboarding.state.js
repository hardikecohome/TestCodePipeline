module.exports('onboarding.state', function () {
    var state = {
        'owner-info': {
            'owner1': {
                
            }
        },
        selectedProvinces: [],
        nextProvinceId: 0
    };

    var constants = {

    };

    window.state = state;

    return {
        state: state,
        constants: constants
    };
});
