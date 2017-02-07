function checkApplicantsAge() {
    var isApprovalAge = false;
    $('.check-age').each(function () {
        //var birthday = Date.parseExact($(this).value, "M/d/yyyy");            
        var birthday = $(this).datepicker('getDate');//Date.parseExact($(this).value, "M/d/yyyy");
        var ageDifMs = Date.now() - birthday.getTime();
        var ageDate = new Date(ageDifMs); // miliseconds from epoch
        var age = Math.abs(ageDate.getUTCFullYear() - 1970);
        if (age <= 75) {
            isApprovalAge = isApprovalAge || true;
        }
    });
    return isApprovalAge;
};