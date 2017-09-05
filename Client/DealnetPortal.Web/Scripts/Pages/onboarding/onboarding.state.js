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
            'insurence-files': [],
            'license': [],
            'addedLicense': [],
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
            requiredFields: ['full-legal-name', 'operating-name', 'company-phone', 'company-email-address', 'company-street', 'company-city', 'company-province',
                'company-postal', 'company-type', 'years-in-business', 'work-provinces']
        },
        'product': {
            selectedEquipment: [],
            nextEquipmentId: 0,
            nextBrandNumber: 0,
            requiredFields: ['primary-brand', 'annual-sales-volume', 'av-transaction-size', ' sales-approach', 'lead-gen', 'program-service', 'relationship', 'equipment', 'reason-for-interest']
        }
    };

    var constants = {
        maxAdditionalOwner: 4,
        maxVoidChequeFiles: 3,
        requiredFields: [
            'firstname', 'lastname', 'email', 'birthdate', 'cellphone', 'street', 'city',
            'province', 'postalcode', 'percentage'
        ],
        validFileExtensions: ['pdf', 'doc', 'docx', 'jpg', 'jpeg', 'png']
    };

    window.state = state;
    window.constants = constants;

    return {
        state: state,
        constants: constants
    };
});
