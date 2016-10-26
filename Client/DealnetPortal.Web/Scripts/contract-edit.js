$(document)
		.ready(function() {
		    $.fn.extend({
		        MyDealFunctionality: function (options) {
		            var defaults = {};
		            var opts = defaults;
		            if (options) {
		                opts = $.extend(defaults, options);
		            }

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
                   
     
   $('body').on('change', '.file-uploaded input[type=file]', function () { 
            $('.file-uploaded .text-center:last').html($('.file-uploaded input[type=file]:last').val().match(/[\w-]+\.\w+/gi));
        });

        $("#save").on('click', function () {
               var n,m = 0;
               $('.file-uploaded form').each(function () {
                       $(this).ajaxForm({
                           method: 'post',
                           contentType: false,
                           beforeSend: function (event) {
                               var percentVal = '0%';                             
                                $('.file-uploaded form').clearForm();
                                $('.file-uploaded form').resetForm();                              
                                $('.file-uploaded .progress-bar').eq(n).width(percentVal);
                                $('.file-uploaded .progress-bar-value').eq(n).html(percentVal);
                           },
                           uploadProgress: function (event, position, total, percentComplete) {
                               var percentVal = percentComplete + '%';
                               $('.file-uploaded .progress-bar').eq(n).width(percentVal);
                               $('.file-uploaded .progress-bar-value').eq(n).html(percentVal);
                               n++;
                           },
                           success: function (result) {
                               m++;
                               if (result.message="success")
                               {
                                   $('.file-uploaded .progress-bar').width(100 + "%");
                                   $('.file-uploaded .progress-bar-value').html(100 + '%');                                
                               }
                               if (m == $('.file-uploaded form').length)
                                   setTimeout(closeModalWindow, 1000); 
                           },
                           complete: function (xhr) {                              
                           },
                           error: function () {
                              
                               $('.file-uploaded form').resetForm();
                               $('.file-uploaded .progress-bar').hide();
                               $('.file-uploaded .progress-bar-value').hide();
                           }
                       }).submit();                      
               });            
              return false;
        });
                 
        var closeModalWindow = function () {           
           var url= opts.Url;
            $('#upload-documents-modal').modal("toggle");
            $('.clear-data-link').click();
            $('.no-documents').hide();
            $.ajax({
                type: "POST",
                url: url,
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                success: success,
                error: error
            });
        };          
            function success(data, status) {
                $('.documents').empty();
                    $.each(data, function (index, value) {
                        $('.documents').append('<li>'+value+'</li>');
                    });
                }         
            function error() {
                $('.documents').append('<div> Occurred error <div>');
            }
                
            }
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