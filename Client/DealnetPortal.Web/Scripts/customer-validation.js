function checkApplicantsAge() {
    var atLeastOneValid = false;
    $('.check-age').each(function () {
        var birthday = $('body').is('.ios-device') ? $(this).val() : $(this).datepicker('getDate');
        if (birthday === null || birthday === undefined || birthday === "") { return true; }

        if (typeof birthday === "string") {
            birthday = Date.parse(birthday);
        }

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

function checkApplicantAgeOnSelect(date) {
    if (date === null || date === undefined) { return true; }
    if (typeof date === "string") {
        date = Date.parse(date);
    }

    var ageDifMs = Date.now() - date.getTime();
    var ageDate = new Date(ageDifMs);
    var age = Math.abs(ageDate.getUTCFullYear() - 1970);

    if (age < 76) {
        return true;
    }

    return false;
}

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
    var mainCustomerAgrees = $('#home-owner-agrees').prop('checked');
    if ($("#additional1-section").data('active')) {
        var additionalCustomerAgrees = $('#additional-owner-agrees').prop('checked');
        return mainCustomerAgrees && additionalCustomerAgrees ? true : false;
    } else {
        return mainCustomerAgrees ? true : false;
    }
}
