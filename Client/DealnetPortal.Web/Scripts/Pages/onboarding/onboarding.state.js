module.exports('onboarding.state', function () {
    var state = {
        'owner-info': {
            'owners': {
                'owner1': {
                    requiredFields: [
                        'firstname', 'lastname', 'email', 'birthdate', 'cellphone', 'street', 'city',
                        'province', 'postalcode'
                    ]
                }
            },
            'nextOwnerIndex': 2

        },
        selectedProvinces: [],
        nextProvinceId: 0
    };

    var constants = {
        maxAdditionalOwner: 5,
        requiredFields: [
            'firstname', 'lastname', 'email', 'birthdate', 'cellphone', 'street', 'city',
            'province', 'postalcode'
        ]
    };

    window.state = state;

    return {
        state: state,
        constants: constants
    };
});
