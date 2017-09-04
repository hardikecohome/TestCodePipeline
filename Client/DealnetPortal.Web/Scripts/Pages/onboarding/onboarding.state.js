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
            'nextOwnerIndex': 0
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
        'company': {
            selectedProvinces: [],
            nextProvinceId: 0,
            requiredFields: ['full-legal-name', 'operating-name', 'company-phone', 'company-email-address', 'company-street', 'company-city', 'company-province', 'company-city',
                'company-postal', 'company-type', 'years-in-business', 'work-provinces']
        },
        selectedEquipment: [],
        nextEquipmentId: 0,
        nextBrandNumber: 0
    };

    var constants = {
        maxAdditionalOwner: 4,
        requiredFields: [
            'firstname', 'lastname', 'email', 'birthdate', 'cellphone', 'street', 'city',
            'province', 'postalcode', 'percentage'
        ]
    };

    window.state = state;
    window.constants = constants;

    return {
        state: state,
        constants: constants
    };
});
