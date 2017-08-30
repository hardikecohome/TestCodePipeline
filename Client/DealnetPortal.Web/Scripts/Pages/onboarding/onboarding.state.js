﻿module.exports('onboarding.state', function () {
    var state = {
        'owner-info': {
            'owners': {
                'owner0': {
                    requiredFields: [
                        'firstname', 'lastname', 'email', 'birthdate', 'cellphone', 'street', 'city',
                        'province', 'postalcode'
                    ]
                }
            },
            'nextOwnerIndex': 1

        },
        selectedProvinces: [],
        selectedEquipment: [],
        nextProvinceId: 0,
        nextEquipmentId: 0,
        nextBrandNumber:1
    };

    var constants = {
        maxAdditionalOwner: 4,
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