module.exports('basicInfo.index', function (require) {
    var BasicInfo = require('basicInfo.component');

    var dob = require('dob-selecters'); var checkApplicantsAge = require('customer-validation').checkApplicantsAge;
    var checkHomeOwner = require('customer-validation').checkHomeOwner;
    var checkCreditAgree = require('customer-validation').checkCreditAgree;
    var scrollPageTo = require('scrollPageTo');

    function init (model) {
        $('.dob-group').each(function (index, el) {
            dob.initDobGroup(el);
        });

        $('.dob-input').on('change', function () {
            $(this).valid();
        });

        var vm = new BasicInfo(model);

        $('#add-additional-applicant').on('click', function () {
            vm.hasAdditional(true);
        });
        $('#additonal1-remove').on('click', function () {
            vm.hasAdditional(false);
        })

        ko.applyBindings(vm, document.getElementById('main-form'));

        $("#save-and-proceed-button").click(function (event) {
            var isApprovalAge = checkApplicantsAge();
            var isHomeOwner = checkHomeOwner();
            var isAgreesToCreditCheck = checkCreditAgree();
            //var quebecDealerValid = isQuebecDealer && vm.showEmployment();

            //if(quebecDealerValid)
            //if (!isApprovalAge) {
            //    $('#age-warning-message').hide();
            //    //$('#age-error-message').show();
            //    //scrollPageTo($('#age-error-message'));
            //}
            if (!vm.allowQcDealerProceed()) {
                $("#proceed-qc-dealer").show();
                scrollPageTo($("#proceed-qc-dealer"));
            }

            if (!isHomeOwner) {
                $("#proceed-homeowner-errormessage").show();
                scrollPageTo($("#borrower-is-homeowner"));
            }

            if (!isAgreesToCreditCheck) {
                $('#proceed-error-message').show();
                scrollPageTo($("#proceed-error-message"));
            }

            if (!vm.valid() || !vm.allowQcDealerProceed() ||!isHomeOwner || !isApprovalAge || !isAgreesToCreditCheck) {
                if ($('#main-form').valid()) {
                    event.preventDefault();
                } else {
                    $('.dob-input').each(function(index, el) {
                        dob.validate(el);
                    });
                }
            }
        });
    }

    return {
        init: init
    };
});
