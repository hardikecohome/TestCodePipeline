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
        'documents': {
            'void-cheque-files': [],
            'insurence-files': []
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
        maxVoidChequeFiles: 3,
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
