module.exports('onboarding.state', function () {
    var state = {
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
