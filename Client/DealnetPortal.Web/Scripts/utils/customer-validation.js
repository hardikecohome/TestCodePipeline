﻿module.exports('customer-validation', function () {

    function checkApplicantsAge () {
        var atLeastOneValid = false;
        $('.check-age').each(function () {
            var birthday = this.value;
            if (birthday === null || birthday === undefined || birthday === "") { return true; }

            if (typeof birthday === "string") {
                birthday = new Date(birthday);
            }

            var ageDifMs = Date.now() - birthday.getTime();
            var ageDate = new Date(ageDifMs);
            var age = Math.abs(ageDate.getUTCFullYear() - 1970);
            //if (age < 76) {
            //    atLeastOneValid = true;
            //    return false;
            //}
            //Please uncomment above code and delete next line to implement age check condition again 
            atLeastOneValid = true;
        });
        return atLeastOneValid;
    };

    function checkApplicantAgeOnSelect (date) {
        if (date === null || date === undefined) { return true; }
        if (typeof date === "string") {
            date = new Date(date);
        }

        var ageDifMs = Date.now() - date.getTime();
        var ageDate = new Date(ageDifMs);
        var age = Math.abs(ageDate.getUTCFullYear() - 1970);

        //if (age < 76) {
        //    return true;
        //}

        //return false;
        return true;
    }

    function checkCreditAgree () {
        var mainCustomerAgrees = $('#home-owner-agrees').prop('checked');
        if ($("#additional1-section").data('active')) {
            var additionalCustomerAgrees = $('#additional-owner-agrees').prop('checked');
            return mainCustomerAgrees && additionalCustomerAgrees ? true : false;
        } else {
            return mainCustomerAgrees ? true : false;
        }
    }
    return {
        checkApplicantsAge: checkApplicantsAge,
        checkApplicantAgeOnSelect: checkApplicantAgeOnSelect,
        checkCreditAgree: checkCreditAgree
    };
});
