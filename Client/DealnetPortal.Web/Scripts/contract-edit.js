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

        $(".add-main-document").on('click', function(){
            var newDocForm = $('.main-document-template').clone(true).removeClass('main-document-template').removeClass('file-uploaded');
            newDocForm.appendTo($('#upload-documents-modal .modal-body .documents-main-group'));
        });
        $(".add-other-document").on('click', function(){
            var newDocForm = $('.other-document-template').clone(true).removeClass('other-document-template').removeClass('file-uploaded');
            newDocForm.appendTo($('#upload-documents-modal .modal-body .documents-other-group'));
        });
        $('#upload-documents-modal .file-upload').on('click', function(){
            $(this).parents('.form-group').addClass('file-uploaded');
            $('.file-uploaded .progress-bar:last').width('0%');
            return true;
        });
        $('.progress-container .clear-data-link').on('click', function(){
            $(this).parents('.form-group').removeClass('file-uploaded');
        });
        });            
   
        $('input[type="file"]').on('change', function () {          
            $('form:last').ajaxForm({
                method: 'post',
                contentType: false,
                beforeSend: function (data) {
                var percentVal = '0%';
                $('.file-uploaded .progress-bar:last').width(percentVal);
                $('.file-uploaded .progress-bar-value:last').html(percentVal);
                $('.file-uploaded .text-center:last').html(event.currentTarget.value.match(/[\w-]+\.\w+/gi));
            },
            uploadProgress: function (event, position, total, percentComplete) {
                var percentVal = percentComplete + '%';
                $('.file-uploaded .progress-bar:last').width(percentVal);
                $('.file-uploaded .progress-bar-value:last').html(percentVal);               
            },
            complete: function (xhr) {
                var percentVal = '0%';
                $('.file-uploaded .progress-bar:last').width(percentVal);
                $('.file-uploaded .progress-bar-value:last').html(percentVal);
                alert(xhr.responseText);
            }
            }).submit();
    });

function assignDatepicker() {
    $(this).datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        yearRange: '1900:2200',
        minDate: Date.parse("1900-01-01")
    });
}