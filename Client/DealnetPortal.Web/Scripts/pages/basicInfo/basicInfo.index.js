module.exports('basicInfo.index', function (require) {
    var homeOwnerEmployment = require('basicInfo.homeOwner.employment');
    var additionalEmployment = require('basicInfo.additionalApplicants.employment');

    var dob = require('dob-selecters');
    var checkApplicantsAge = require('customer-validation').checkApplicantsAge;
    var checkHomeOwner = require('customer-validation').checkHomeOwner;
    var checkCreditAgree = require('customer-validation').checkCreditAgree;
    var scrollPageTo = require('scrollPageTo');

    function init () {
        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });

        $('#save-and-proceed-button').click(function (event) {
            var isApprovalAge = checkApplicantsAge();
            var isHomeOwner = checkHomeOwner();
            var isAgreesToCreditCheck = checkCreditAgree();
            var quebecDealerValid = $('#is-quebec-dealer').val().toLowerCase() === 'true'
                ? $('#administrative_area_level_1').val().toLowerCase() !== 'qc'
                : true;

            //if(quebecDealerValid)
            //if (!isApprovalAge) {
            //    $('#age-warning-message').hide();
            //    //$('#age-error-message').show();
            //    //scrollPageTo($('#age-error-message'));
            //}
            if (!quebecDealerValid) {
                $('#proceed-qc-dealer').show();
                scrollPageTo($('#proceed-qc-dealer'));
            }

            if (!isHomeOwner) {
                $('#proceed-homeowner-errormessage').show();
                scrollPageTo($('#borrower-is-homeowner'));
            }

            if (!isAgreesToCreditCheck) {
                $('#proceed-error-message').show();
                scrollPageTo($('#proceed-error-message'));
            }

            if (!quebecDealerValid || !isHomeOwner || !isApprovalAge || !isAgreesToCreditCheck) {
                if ($('#main-form').valid()) {
                    event.preventDefault();
                } else {
                    $('.dob-input').each(function(index, el) {
                        dob.validate(el);
                    });
                }
            }
        });

        var province = $('#administrative_area_level_1');
        var addAdditionalButton = $('#add-additional-applicant');
        var additionalSection = $('#additional1-section');

        homeOwnerEmployment.initEmployment(province.val().toLowerCase()!=='qc');
        additionalEmployment.initEmployment(province.val().toLowerCase()!=='qc' || additionalSection.is(':hidden'));

        province.on('change', function(e) {
            if (e.target.value.toLowerCase() === 'qc') {
                homeOwnerEmployment.enableEmployment();
                if (!additionalSection.is(':hidden')) {
                    additionalEmployment.enableEmployment();
                }
            } else {
                homeOwnerEmployment.disableEmployment();
                if (!additionalSection.is(':hidden')) {
                    additionalEmployment.disableEmployment();
                }
            }
        });

        addAdditionalButton.on('click', function() {
            if ($('#administrative_area_level_1').val().toLowerCase() == 'qc') {
                additionalEmployment.enableEmployment();
            }
        });

        $('#additional1-remove').click(function () {
            additionalEmployment.disableEmployment();
        });
    }

    return {
        init: init
    };
});
