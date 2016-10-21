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
            expandReplies(this);
            return false;
        });

        $('.comment .write-reply-link').on('click', addReplyFrom);

        $('.reply-button').on('click', function () {
            var form = $(this).parents('form');
            submitComment(form, addBaseComment);
            return false;
        });

        $('.comment .comment-remove-link').on('click', function () {
            removeComment(this);
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
                alert(xhr.responseText);
            }
            }).submit();
    });

function addReplyFrom() {
    var currComment = $(this).parents('.comment');
    var existingForm = currComment.find('.comment-reply-form');
    if (existingForm.length) {
        existingForm.remove();
    } else {
        var commentForm = $('#comment-reply-form').clone();
        commentForm.find("input[name='ParentCommentId']").val(currComment.find("input[name='comment-id']").val());
        commentForm.attr("id", "");
        commentForm.appendTo(currComment);
        commentForm.find('.reply-button').on('click', function () {
            var form = $(this).parents('form');
            submitComment(form, addChildComment);
            return false;
        });
    }
    return false;
}

function removeComment(button) {
    var commentId = $(button).siblings("input[name='comment-id']").val();
    submitCommentRemoval($(button).parents('.comment'), commentId);
}

function expandReplies(button) {
    $(button).toggleClass('active');
    $(button).parents('.comment').toggleClass('active');
}

function assignDatepicker() {
    $(this).datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        yearRange: '1900:2200',
        minDate: Date.parse("1900-01-01")
    });
}

function submitComment(form, addComment) {
    showLoader();
    form.ajaxSubmit({
        type: "POST",
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while adding comment");
            } else {
                var comment = addComment(form, json);
                comment.find('.write-reply-link').on('click', addReplyFrom);
                comment.find('.show-comments-answers').on('click', function () {
                    expandReplies(this);
                    return false;
                });
                comment.find('.comment-remove-link').on('click', function () {
                    var commentId = $(this).siblings("input[name='comment-id']").val();
                    submitCommentRemoval($(this).parents('.comment'), commentId);
                    return false;
                });
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}

function submitCommentRemoval(comment, commentId) {
    showLoader();
    var form = $('#remove-comment-form');
    form.find("input[name='commentId']").val(commentId);
    form.ajaxSubmit({
        type: "POST",
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while removing comment");
            } else {
                var parentComment = comment.parents('ul').prev('.comment');
                var replies = comment.next('ul');
                comment.remove();
                replies.remove();
                var prevQuantity = parentComment.find('.answers-quantity');
                var prevQuantityVal = Number(prevQuantity.text());
                prevQuantity.text(prevQuantityVal - 1);
                if (prevQuantityVal === 1) {
                    parentComment.find('.show-comments-answers').toggleClass('active');
                    parentComment.toggleClass('active');
                }
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}

function addBaseComment(form, json) {
    var currComments = $('#comments-start');
    var commentTemplate = $('#comment-template').clone();
    commentTemplate.find(".comment-body input[name='comment-id']").val(json.updatedCommentId);
    var commentText = $('.base-comment-text');
    commentTemplate.find(".comment-body p").text(commentText.val());
    commentText.val("");
    commentTemplate.find(".comment-user .comment-username").text(form.parents('.comments-widget').data('username'));
    commentTemplate.find(".comment-update-time").text(new Date().toString("hh:mm tt M/d/yyyy"));
    commentTemplate.attr("id", "");
    commentTemplate.insertAfter(currComments);
    var replies = $('#replies-template').clone();
    replies.insertAfter(commentTemplate);
    replies.attr("id", "");
    return commentTemplate;
}

function addChildComment(form, json) {
    var parentComment = form.parents('.comment');
    var currComments = parentComment.next('ul').find('li:first');
    var commentTemplate = $('#comment-template').clone();
    commentTemplate.find(".comment-body input[name='comment-id']").val(json.updatedCommentId);
    commentTemplate.find(".comment-body p").text(form.find('.base-comment-text').val());
    commentTemplate.find(".comment-user .comment-username").text(form.parents('.comments-widget').data('username'));
    commentTemplate.find(".comment-update-time").text(new Date().toString("hh:mm tt M/d/yyyy"));
    commentTemplate.attr("id", "");
    commentTemplate.appendTo(currComments);
    var replies = $('#replies-template').clone();
    replies.appendTo(currComments);
    replies.attr("id", "");
    form.parents('.comment-reply-form').remove();
    var prevQuantity = parentComment.find('.answers-quantity');
    var prevQuantityVal = Number(prevQuantity.text());
    prevQuantity.text(prevQuantityVal + 1);
    if (prevQuantityVal === 0) {
        parentComment.find('.show-comments-answers').toggleClass('active');
        parentComment.toggleClass('active');
    }
    return commentTemplate;
}