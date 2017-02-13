function checkApplicantsAge() {
    var atLeastOneValid = false;
    $('.check-age').each(function () {
        var birthday = $(this).datepicker('getDate');
        if (birthday === null) { return true; }
        var ageDifMs = Date.now() - birthday.getTime();
        var ageDate = new Date(ageDifMs);
        var age = Math.abs(ageDate.getUTCFullYear() - 1970);
        if (age <= 75) {
            atLeastOneValid = true;
            return false;
        }
    });
    return atLeastOneValid;
};