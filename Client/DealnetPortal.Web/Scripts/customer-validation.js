function checkApplicantsAge() {
    var atLeastOneValid = false;
    $('.check-age').each(function () {
        var birthday = $(this).datepicker('getDate');
        if (birthday === null) { return true; }
        var ageDifMs = Date.now() - birthday.getTime();
        var ageDate = new Date(ageDifMs);
        var age = Math.abs(ageDate.getUTCFullYear() - 1970);
        if (age < 76) {
            atLeastOneValid = true;
            return false;
        }
    });
    return atLeastOneValid;
};

function checkHomeOwner() {
    var isHomeOwner = false;
    $('.check-homeowner').each(function () {
        if ($(this).prop('checked')) {
            isHomeOwner = true;
            return false;
        }
    });
    return isHomeOwner;
};

function checkCreditAgree() {
    var mainCustomerAgrees = $('#home-owner-agrees').prop('checked') && $('#agreement-checkbox-data2').prop('checked');
    if ($("#additional1-section").data('active')) {
        var additionalCustomerAgrees = $('#additional-owner-agrees').prop('checked') && $('#additional-agreement-checkbox-data2').prop('checked');
        return mainCustomerAgrees && additionalCustomerAgrees ? true : false;
    } else {
        return mainCustomerAgrees ? true : false;
    }
    
}
