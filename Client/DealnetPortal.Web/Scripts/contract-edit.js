$(document)
		.ready(function() {

        $('.date-input').each(assignDatepicker);
        var initPaymentTypeForm = $("#payment-type-form").find(":selected").val();
        managePaymentFormElements(initPaymentTypeForm);
        $("#payment-type-form").change(function () {
            managePaymentFormElements($(this).find(":selected").val());
        });

        $('.comment .show-comments-answers').on('click', function() {
            expandReplies(this);
            return false;
        });

        $('.comment .write-reply-link').on('click', addReplyFrom);

        $('.reply-button').on('click', function () {
            var form = $(this).parent().closest('form');
            submitComment(form, addBaseComment);
            return false;
        });

        if (!isMobileBrowser) {
            $('.base-comment-text').keypress(function(e) {
                if (e.which == 13 && !e.shiftKey) {
                    var form = $(this).parent().closest('form');
                    submitComment(form, addBaseComment);
                }
            });
        }

        $('.comment .comment-remove-link').on('click', function() {
            removeComment(this);
            return false;
        });

        $(".add-main-document").on('click', function() {
            var newDocForm = $('.main-document-template').clone(true).removeClass('main-document-template').removeClass('file-uploaded');
            newDocForm.appendTo($('#upload-documents-modal .modal-body .documents-main-group'));
        });
        $(".add-other-document").on('click', function() {
            var newDocForm = $('.other-document-template').clone(true).removeClass('other-document-template').removeClass('file-uploaded');
            newDocForm.appendTo($('#upload-documents-modal .modal-body .documents-other-group'));
        });
        $('#upload-documents-modal .file-upload').on('click', function() {
            $(this).parent().closest('.form-group').addClass('file-uploaded');
            $('.file-uploaded .progress-bar:last').width('0%');
            return true;
        });
        $('.progress-container .clear-data-link').on('click', function() {
            $(this).parent().closest('.form-group').removeClass('file-uploaded');
        });

        $('#main-documents .remove-link').on('click', function () {
            removeDocument(this, false);
        });

        $('#other-documents .remove-link').on('click', function () {
            removeDocument(this, true);
        });

        $('body').on('change', '.file-uploaded input[type=file]', function() {
            $('.file-uploaded .text-center:last').html($('.file-uploaded input[type=file]:last').val().match(/[\w-]+\.\w+/gi));
        });

        var closeModalWindow = function() {
            $('#upload-documents-modal').modal("toggle");
            $('.clear-data-link').click();
            //$('.no-documents').hide();
            $('#upload-success').show();
        };

        $("#save").on('click', function() {
            var m = 0;
            $('.file-uploaded form').each(function() {
                var form = $(this);
                form.ajaxSubmit({
                    method: 'post',
                    contentType: false,
                    beforeSend: function(event) {
                        form.find('.progress-container .clear-data-link').on('click', function () {
                            event.abort();
                        });
                        var percentVal = '0%';
                        // $('.file-uploaded form').clearForm();
                        //  $('.file-uploaded form').resetForm();                              
                        form.find('.file-uploaded .progress-bar').width(percentVal);
                        form.find('.file-uploaded .progress-bar-value').html(percentVal);
                    },
                    uploadProgress: function(event, position, total, percentComplete) {
                        var percentVal = percentComplete + '%';
                        form.find('.progress-bar').width(percentVal);
                        form.find('.progress-bar-value').html(percentVal);
                    },
                    success: function(result) {
                        if (result.isSuccess) {
                            form.find('.progress-bar').width(100 + "%");
                            form.find('.progress-bar-value').html(100 + '%');
                            var typeId = form.find('.document-type-id');
                            if (typeId.length) {
                                var mainDocument = $('#main-documents #document-type-' + typeId.val());
                                mainDocument.find("input[name='documentId']").val(result.updatedDocumentId);
                                mainDocument.find('.file-name').text(form.find("input[name='File']").val().match(/[\w-]+\.\w+/gi));
                                mainDocument.find('.remove-link').show();
                            } else {
                                //For "Other" document type
                                var document = $('#other-document-template').clone();
                                document.attr("id", "");
                                document.find("input[name='documentId']").val(result.updatedDocumentId);
                                document.find('.file-name').text(form.find("input[name='DocumentName']").val());
                                document.appendTo('#other-documents');
                                document.find('.remove-link').on('click', function () {
                                    removeDocument(this, true);
                                });
                            }
                        } else {
                            if (result.isError) {
                                alert("An error occurred while uploading file");
                            }
                        }
                    },
                    complete: function (xhr) {
                        m++;
                        if (m === $('.file-uploaded form').length)
                            setTimeout(closeModalWindow, 1000);
                    },
                    error: function() {
                        form.find('form').resetForm();
                        form.find('.progress-bar').hide();
                        form.find('.progress-bar-value').hide();
                    }
                });
            });
            return false;
        });
        $('.main-document-upload').change(function() {
            var input = $(this);
            var form = input.parent().closest('form');
            var tabContainers = $('.documents-container .' + form.data('container'));
            var documentNaming = form.find('.document-naming');
            var progressContainer = form.find('.progress-container');
            var progressBar = form.find('.progress-bar');
            var progressBarValue = form.find('.progress-bar-value');
            var errorDesc = form.find('.error-descr');
            var prevDocumentName;
            var wasCancelled;
            var afterError = function(message) {
                form.find('.error-message').text(message || 'An error occurred while uploading file.');
                errorDesc.show();
                documentNaming.text(prevDocumentName);
                tabContainers.removeClass('uploaded');
                tabContainers.addClass('error');
            };
            var operatGuid = generateGuid();
            form.find("input[name='OperationGuid']").val(operatGuid);
            form.ajaxSubmit({
                method: 'post',
                contentType: false,
                beforeSend: function (event) {
                    //tabContainers.removeClass('uploaded');
                    prevDocumentName = documentNaming.text();
                    var cancelButton = form.find('.progress-container .clear-data-link');
                    cancelButton.off('click');
                    cancelButton.on('click', function () {
                        showLoader();
                        wasCancelled = true;
                        //event.abort();
                        var cancelUploadForm = $('#cancel-upload-form');
                        cancelUploadForm.find("input[name='operationGuid']").val(operatGuid);
                        cancelUploadForm.ajaxSubmit();
                    });
                    var percentVal = '0%';                              
                    progressBar.width(percentVal);
                    progressBarValue.html(percentVal);
                    documentNaming.text(input.val().match(/[\w-]+\.\w+/gi));
                    progressContainer.show();
                },
                uploadProgress: function(event, position, total, percentComplete) {
                    var percentVal = percentComplete + '%';
                    progressBar.width(percentVal);
                    progressBarValue.html(percentVal);
                },
                success: function (result) {
                    //if (wasCancelled){ return; }
                    if (result.isSuccess) {
                        progressBar.width(100 + "%");
                        progressBarValue.html(100 + '%');
                        errorDesc.hide();
                        tabContainers.removeClass('error');
                        tabContainers.addClass('uploaded');
                    } else if (result.isError) {
                        afterError(result.errorMessage);
                    } else if (result.wasCancelled) {
                        documentNaming.text(prevDocumentName);
                    }
                },
                complete: function (xhr) {
                    progressContainer.hide();
                    input.val('');
                    hideLoader();
                },
                error: function () {
                    if (!wasCancelled) {
                        afterError();
                    }
                }
            });
        });
    });

function generateGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function managePaymentFormElements(paymentType) {
    switch (paymentType) {
        case '0':
            $(".pap-payment-form").hide();
            $(".enbridge-payment-form").show();
            break;
        case '1':
            $(".enbridge-payment-form").hide();
            $(".pap-payment-form").show();
            break;
    }
}

function removeDocument(button, removeWholeLine) {
    $("#remove-document-form input[name='documentId']").val($(button).prev("input[name='documentId']").val());
    submitDocumentRemoval($(button), removeWholeLine);
}

function submitDocumentRemoval(removalLink, removeWholeLine) {
    showLoader();
    var form = $('#remove-document-form');
    form.ajaxSubmit({
        type: "POST",
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while removing document");
            } else {
                if (json.isSuccess) {
                    if (removeWholeLine) {
                        removalLink.parent().closest('.document-row').remove();
                    } else {
                        removalLink.hide();
                        removalLink.next(".dealnet-info-link").text("");
                    }
                }
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}

function addReplyFrom() {
    var currComment = $(this).parent().closest('.comment');
    var existingForm = currComment.find('.comment-reply-form');
    if (existingForm.length) {
        existingForm.remove();
    } else {
        var commentForm = $('#comment-reply-form').clone();
        commentForm.find("input[name='ParentCommentId']").val(currComment.find("input[name='comment-id']").val());
        commentForm.attr("id", "");
        commentForm.appendTo(currComment);
        commentForm.find('textarea').text('');
        resetPlacehoder(commentForm.find('textarea'));
        commentForm.find('.reply-button').on('click', function () {
            var form = $(this).parent().closest('form');
            submitComment(form, addChildComment);
            return false;
        });
        if (!isMobileBrowser) {
            commentForm.find('.base-comment-text').keypress(function(e) {
                if (e.which == 13 && !e.shiftKey) {
                    var form = $(this).parent().closest('form');
                    submitComment(form, addChildComment);
                }
            });
        }
    }

    return false;
}

function removeComment(button) {
    var commentId = $(button).siblings("input[name='comment-id']").val();
    submitCommentRemoval($(button).parent().closest('.comment'), commentId);
}

function expandReplies(button) {
    $(button).toggleClass('active');
    $(button).parent().closest('.comment').toggleClass('active');
}

function assignDatepicker() {
    var input = $(this);
    inputDateFocus(input);
    input.datepicker({
        dateFormat: 'mm/dd/yy',
        changeYear: true,
        changeMonth: (viewport().width < 768) ? true : false,
        yearRange: '1900:2200',
        minDate: new Date(),
        onClose: function(){
            onDateSelect($(this));
        }
    });
}

function submitComment(form, addComment) {
    if (!form.find('.base-comment-text').val().trim()) { return; }
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
                    submitCommentRemoval($(this).parent().closest('.comment'), commentId);
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
                var parentComment = comment.parent().closest('ul').prev('.comment');
                var replies = comment.next('ul');
                comment.remove();
                replies.remove();
                var prevQuantity = parentComment.find('.answers-quantity');
                var prevQuantityVal = Number(prevQuantity.text());
                prevQuantity.text(prevQuantityVal - 1);
                if (prevQuantityVal === 1) {
                    parentComment.find('.show-comments-answers').toggleClass('active');
                    parentComment.toggleClass('active');
                    if (parentComment.find('.owner-comment').length) {
                        parentComment.find('.comment-remove-link').show();
                    }
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
    commentTemplate.find(".comment-body p").html(commentText.val().replace(/\r?\n/g, '<br />'));
    commentText.val("");
    commentTemplate.find(".comment-user .comment-username").text(form.parent().closest('.comments-widget').data('username'));
    commentTemplate.find(".comment-update-time").text(new Date().toString("hh:mm tt M/d/yyyy"));
    commentTemplate.attr("id", "");
    commentTemplate.insertAfter(currComments);
    var replies = $('#replies-template').clone();
    replies.insertAfter(commentTemplate);
    replies.attr("id", "");
    return commentTemplate;
}

function addChildComment(form, json) {
    var parentComment = form.parent().closest('.comment');
    var currComments = parentComment.next('ul').find('li:first');
    var commentTemplate = $('#comment-template').clone();
    commentTemplate.find(".comment-body input[name='comment-id']").val(json.updatedCommentId);
    commentTemplate.find(".comment-body p").html(form.find('.base-comment-text').val().replace(/\r?\n/g, '<br />'));
    commentTemplate.find(".comment-user .comment-username").text(form.parent().closest('.comments-widget').data('username'));
    commentTemplate.find(".comment-update-time").text(new Date().toString("hh:mm tt M/d/yyyy"));
    commentTemplate.attr("id", "");
    commentTemplate.appendTo(currComments);
    var replies = $('#replies-template').clone();
    replies.appendTo(currComments);
    replies.attr("id", "");
    form.parent().closest('.comment-reply-form').remove();
    var prevQuantity = parentComment.find('.answers-quantity');
    var prevQuantityVal = Number(prevQuantity.text());
    prevQuantity.text(prevQuantityVal + 1);
    if (prevQuantityVal === 0) {
        parentComment.find('.show-comments-answers').toggleClass('active');
        parentComment.toggleClass('active');
        parentComment.find('.comment-remove-link').hide();
    }
    return commentTemplate;
}

function assignAutocompletes() {
    $(document)
        .ready(function () {
            initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
            initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
            for (var i = 1; i <= 3; i++) {
                initGoogleServices("additional-street-" + i, "additional-locality-" + i, "additional-administrative_area_level_1-" + i, "additional-postal_code-" + i);
            }
        });
}