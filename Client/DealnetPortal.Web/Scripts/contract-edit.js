$(document)
		.ready(function() {
		    $('.date-input').each(assignDatepicker);
            $("#home-phone").rules("add", "required");
    $("#cell-phone").rules("add", "required");
    $("#bank-number").rules("add", "required");
    $("#transit-number").rules("add", "required");
    $("#account-number").rules("add", "required");
    $("#enbridge-gas-distribution-account").rules("add", "required");
    $("#meter-number").rules("add", "required");
    //
    setValidationRelation($("#home-phone"), $("#cell-phone"));
    setValidationRelation($("#cell-phone"), $("#home-phone"));
    setValidationRelation($("#enbridge-gas-distribution-account"), $("#meter-number"));
    setValidationRelation($("#meter-number"), $("#enbridge-gas-distribution-account"));

        $('.comment .show-comments-answers').on('click', function() {
            $(this).toggleClass('active');
            $(this).parents('.comment').toggleClass('active');
            return false;
        });


        $('.comment .write-reply-link').on('click', function() {
            var currComment = $(this).parents('.comment');
            var commentForm = $('.comment-reply-form').detach();
            commentForm.appendTo(currComment);
            return false;
        });
    });

function assignDatepicker() {
    $(this).datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        yearRange: '1900:2200',
        minDate: Date.parse("1900-01-01")
    });
}