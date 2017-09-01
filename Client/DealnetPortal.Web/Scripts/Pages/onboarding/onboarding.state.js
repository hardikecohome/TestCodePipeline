module.exports('onboarding.state', function () {
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
        'aknowledgment': {
            'owners': {
                'owner0': {

                }
            }
        },
        'consent': {
            'creditAgreement': false,
            'contactAgreement': false
        },
        selectedProvinces: [],
        nextProvinceId: 0,
        selectedEquipment: [],
        nextEquipmentId: 0,
        nextBrandNumber: 0
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
