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
