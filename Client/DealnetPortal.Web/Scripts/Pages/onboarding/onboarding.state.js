module.exports('onboarding.state', function () {
    var state = {
        'owner-info': {
            'owner1': {
                requiredFields: ['firstname', 'lastname', 'email', 'birthdate', 'homephone', 'cellphone', 'street', 'city', 'province', 'postalcode' ]
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
