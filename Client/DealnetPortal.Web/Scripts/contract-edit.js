﻿configInitialized
    .then(function () {
        togglePrintButton(checkUrl);

        $('.contract-tab').on('click', function (e) {
            e.preventDefault();
            $(this).tab('show');
        });
        $('[id^="signee-btn-"]').on('click', submitOneSignature);
        $('#submit-digital').on('click', submitDigital);

        $('#print-button').on('click', printContract(downloadUrl));
        $('#print-signed-button').on('click', printContract(downloadSignedUrl));

        $('.date-input').each(function (index, input) {
            assignDatepicker(input,
                {
                    yearRange: '1900:2200',
                    minDate: ($(input).hasClass('exlude-min-date')) ? new Date("1900-01-01") : new Date()
                });
        });

        var initPaymentTypeForm = $("#payment-type-form").find(":selected").val();
        managePaymentFormElements(initPaymentTypeForm);
        $("#payment-type-form").change(function () {
            managePaymentFormElements($(this).find(":selected").val());
        });

        $('.comment .show-comments-answers').on('click', function () {
            expandReplies(this);
            return false;
        });

        $('.comment .write-reply-link').on('click', addReplyFrom);

        $('.reply-button').on('click', function () {
            var form = $(this).parent().closest('form');
            submitComment(form, addBaseComment);
            return false;
        });

        checkSubmitAllDocumentsAvailability();

        if (!isMobileBrowser) {
            $('.base-comment-text').keypress(function (e) {
                if (e.which == 13 && !e.shiftKey) {
                    var form = $(this).parent().closest('form');
                    submitComment(form, addBaseComment);
                }
            });
        }

        $('.comment .comment-remove-link').on('click', function () {
            removeComment(this);
            return false;
        });

        var validateFileSize = function (errorAction) {
            try {
                if (this.files[0].size > maxFileUploadSize) {
                    errorAction(translations['MaximumFileSize']);
                    $(this).val('');
                    return false;
                }
                return true;
            } catch (e) {
                console.log("Browser doesn't support modern file size detection feature");
                return false;
            }
        };

        $('.main-document-upload').change(function () {
            var input = $(this);
            var form = input.parent().closest('form');
            var tabContainers = $('.documents-container .' + form.data('container'));
            var documentNaming = form.find('.document-naming');
            var documentItem = form.find('.document-item');
            var progressContainer = form.find('.progress-container');
            var progressBar = form.find('.progress-bar');
            var progressBarValue = form.find('.progress-bar-value');
            var errorDesc = form.find('.error-descr');
            var prevDocumentName = documentNaming.text();;
            var wasCancelled;
            var afterError = function (message) {
                //form.find('.error-message').text(message || translations['ErrorWhileUploadingFile']);
                form.find('.error-message').text(translations['ErrorWhileUploadingFile']);
                errorDesc.show();
                documentNaming.text(prevDocumentName);
                if (!prevDocumentName) {
                    documentItem.hide();
                }
                tabContainers.removeClass('uploaded');
                tabContainers.addClass('error');
            };
            if (!validateFileSize.call(this, afterError)) {
                return;
            }
            var operatGuid = generateGuid();
            form.find("input[name='OperationGuid']").val(operatGuid);
            form.ajaxSubmit({
                method: 'post',
                contentType: false,
                beforeSend: function (event) {
                    //tabContainers.removeClass('uploaded');
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
                    documentNaming.text(input.val().replace(/^.*[\\\/]/, ''));
                    documentItem.show();
                    progressContainer.show();
                },
                uploadProgress: function (event, position, total, percentComplete) {
                    if (percentComplete === 100) {
                        percentComplete = 99;
                    }
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
                        checkSubmitAllDocumentsAvailability();
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
        var submitOtherDocument = function () {
            var input = $(this);
            var form = input.parent().closest('form');
            var tabContainers = $('.documents-container .tab-container-7');
            var progressContainer = form.find('.progress-container');
            var progressBar = form.find('.progress-bar');
            var progressBarValue = form.find('.progress-bar-value');
            var errorDesc = form.find('.error-descr');
            var wasCancelled;
            var afterError = function (message) {
                form.find('.error-message').text(translations['ErrorWhileUploadingFile']);
                errorDesc.show();
                tabContainers.removeClass('uploaded');
                tabContainers.addClass('error');
            };
            if (!validateFileSize.call(this, afterError)) {
                return;
            }
            var operatGuid = generateGuid();
            form.find("input[name='OperationGuid']").val(operatGuid);
            form.ajaxSubmit({
                method: 'post',
                contentType: false,
                beforeSend: function (event) {
                    //tabContainers.removeClass('uploaded');
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
                    progressContainer.show();
                },
                uploadProgress: function (event, position, total, percentComplete) {
                    if (percentComplete === 100) {
                        percentComplete = 99;
                    }
                    var percentVal = percentComplete + '%';
                    progressBar.width(percentVal);
                    progressBarValue.html(percentVal);
                },
                success: function (result) {
                    //if (wasCancelled){ return; }
                    if (result.isSuccess) {
                        form.find("input[name='Id']").val(result.updatedDocumentId);
                        progressBar.width(100 + "%");
                        progressBarValue.html(100 + '%');
                        errorDesc.hide();
                        tabContainers.removeClass('error');
                        tabContainers.addClass('uploaded');
                    } else if (result.isError) {
                        afterError(result.errorMessage);
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
        }
        $('.uploaded-other-document-input').change(submitOtherDocument);
        $('#other-documents-upload').change(function () {
            var documentNameInput = $('#other-documents-name');
            var documentName = documentNameInput.val();
            if (!documentName) {
                $(this).val('');
                $('#empty-document-name-message').show();
                return;
            } else {
                $('#empty-document-name-message').hide();
            }
            var otherDocumentForm = $('#other-document-template').clone();
            otherDocumentForm.find("input[name='DocumentName']").val(documentName);
            otherDocumentForm.find('span.document-naming').text(documentName);
            var blankFileInput = $(this).clone(true);
            blankFileInput.val('');
            $('#other-documents-upload-button').append(blankFileInput);
            documentNameInput.val('');
            var fileInput = $(this).detach();
            fileInput.attr("id", "");
            otherDocumentForm.find('.file-upload').append(fileInput);
            otherDocumentForm.attr("id", "");
            $('#other-documents-list').append(otherDocumentForm);
            fileInput.off('change');
            fileInput.change(submitOtherDocument);
            fileInput.change();
        });

        $('#send-all-documents-report').on('click', auditConfirmModal);
    });

function generateGuid () {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function managePaymentFormElements (paymentType) {
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

function auditConfirmModal () {
    var data = {
        class: "audit-alert-modal",
        message: translations['DidYouUploadAllDocuments'],
        title: translations['FinalCheck'],
        confirmBtnText: translations['Proceed']
    };
    dynamicAlertModal(data);
    $('#confirmAlert').on('click', function () {
        submitAllDocumentsUploaded();

        ga('send', 'event', 'Button', 'Click', 'SendDocuments');
    });
}

function submitAllDocumentsUploaded () {
    showLoader();
    $('#all-documents-uploaded-form').ajaxSubmit({
        method: 'post',
        success: function (result) {
            if (result.isSuccess) {
                $('.before-all-documents-submitted').hide();
                $('#all-documents-submitted-message').show();
                $('.disablable').addClass('disabled');
                $('button.disabled, input.disabled').attr('disabled', 'disabled');
                $('.dealnet-section-edit-link').hide();
                isSentToAudit = true;
            } else if (result.isError) {
                alert(translations['AnErrorWhileSendingReport']);
            }
        },
        error: function () {
        },
        complete: function (xhr) {
            hideLoader();
            hideDynamicAlertModal();
        }
    });
}

function checkSubmitAllDocumentsAvailability () {
    var submitEnabled = true;
    $('.mandatory').each(function () {
        submitEnabled = submitEnabled && $(this).hasClass('uploaded');
    });
    if (submitEnabled && !isSentToAudit) {
        $('button.disabled, input.disabled').removeClass('disabled');
        $('button.disabled, input.disabled').removeProp('disabled');
        $('.before-all-documents-submitted').hide();
    } else {
        if (isSentToAudit) {
            $('.before-all-documents-submitted').hide();
            $('#all-documents-submitted-message').show();
        }
    }
}

function addReplyFrom () {
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
            commentForm.find('.base-comment-text').keypress(function (e) {
                if (e.which == 13 && !e.shiftKey) {
                    var form = $(this).parent().closest('form');
                    submitComment(form, addChildComment);
                }
            });
        }
    }

    return false;
}

function removeComment (button) {
    var commentId = $(button).siblings("input[name='comment-id']").val();
    submitCommentRemoval($(button).parent().closest('.comment'), commentId);
}

function expandReplies (button) {
    $(button).toggleClass('active');
    $(button).parent().closest('.comment').toggleClass('active');
}

function submitComment (form, addComment) {
    if (!form.find('.base-comment-text').val().trim()) { return; }
    showLoader();
    form.ajaxSubmit({
        type: "POST",
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert(translations['ErrorWhileAddingComment']);
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

function submitCommentRemoval (comment, commentId) {
    showLoader();
    var form = $('#remove-comment-form');
    form.find("input[name='commentId']").val(commentId);
    form.ajaxSubmit({
        type: "POST",
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert(translations['ErrorWhileRemovingComment']);
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

function addBaseComment (form, json) {
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

function addChildComment (form, json) {
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

function assignAutocompletes () {
    $(document)
        .ready(function () {
            initGoogleServices("street", "locality", "administrative_area_level_1", "postal_code");
            initGoogleServices("mailing_street", "mailing_locality", "mailing_administrative_area_level_1", "mailing_postal_code");
            for (var i = 1;i <= 3;i++) {
                initGoogleServices("additional-street-" + i, "additional-locality-" + i, "additional-administrative_area_level_1-" + i, "additional-postal_code-" + i);
            }
        });
}

function printCertificate (checkUrl, form) {

    $.get({
        type: "POST",
        url: checkUrl,
        success: function (json) {
            if (json == true) {
                $('#certificate-error-message').hide();
                form.validate();
                if (!form.valid()) {
                    return;
                };
                form.ajaxSubmit({
                    url: form.attr("action"),
                    method: form.attr("method"),  // post
                    success: function (json) {
                        if (json != null) {
                            window.location = json;
                        } else {
                            $('#certificate-error-message').show();
                        }
                    },
                    error: function (xhr, status, p3) {
                        $('#certificate-error-message').show();
                    }
                });
            } else {
                $('#certificate-error-message').show();
            }
        },
        error: function (xhr, status, p3) {
            $('#certificate-error-message').show();
        }
    });
}

function esignatureModel (signers) {
    return JSON.stringify({
        __RequestVerificationToken: $('#signer-form input[name="__RequestVerificationToken"]').val(),
        ContractId: $('#contract-id').val(),
        HomeOwnerId: $('#home-owner-id').val(),
        Status: $('#signature-status').val(),
        Signers: signers
    });
}

function submitDigital (e) {
    var status = $('#signature-status').val().toLowerCase();
    if (status === 'sent')
        cancelSignatures(e);
    else
        submitAllEsignatures(e);
}

function cancelSignatures (e) {
    e.preventDefault();
    var data = {
        message: translations['AreYouSureCancelEsignature'] + '?',
        title: translations['Cancel'],
        confirmBtnText: translations['Proceed']
    };
    dynamicAlertModal(data);
    $('#confirmAlert').one('click', function () {
        hideDynamicAlertModal();
        showLoader();
        var $form = $('#cancel-signature-form');
        $form.find('#contractId').val($('#contract-id').val());
        $form.ajaxSubmit({
            type: 'POST',
            success: function (data) {
                if (!data.isError) {
                    $('#signer-form [id^="signer-status-"]').addClass('hidden');
                    $('#signer-form #submit-digital').text(translations['SendInvites']);
                    $('#signature-status').val('');
                }
                hideLoader();
            },
            error: function (xhr, status, result) {
                console.log(result);
                hideLoader();
            }
        });
    });
}

function submitOneSignature (e) {
    e.preventDefault();
    var $row = $(e.target).parents('.signer-row');
    var email = $row.find('#signer-email-' + rowId);
    if (email.valid()) {
        showLoader();
        var rowId = $row.find('#row-id');
        var signer = [{
            Id: $row.find('#signer-id-' + rowId).val(),
            SignatureStatus: $row.find('#signer-status-' + rowId).val(),
            StatusLastUpdateTime: $row.find('#signer-update-' + rowId).val(),
            Role: $row.find('#signer-role-' + rowId).val(),
            CustomerId: $row.find('#signer-customer-id-' + rowId).val(),
            Email: email.val()
        }];
        $.ajax({
            url: updateEsignUrl,
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: esignatureModel(signers)
        }).done(function (data) {
        }).fail(function (xhr, status, result) {
            console.log(result);
        }).always(function () {
            var data = {
                message: translations['InvitesWereSentToEmails'],
                confirmBtnText: translations['Proceed']
            };
            dynamicAlertModal(data);
            var updateDate = new Date();
            $row.find('.signature-date-hold')
                .text((updateDate.getMonth() + 1) + '/' + updateDate.getDate() + '/' + updateDate.getFullYear() + ' ' + updateDate.getHours() + ':' + updateDate.getMinutes() + ' ' + (updateDate.getHours() > 11 ? 'PM' : 'AM'));
            hideLoader();
        });
    }
}

var statusMap = {
    '0': 'notinitiated',
    '1': 'created',
    '2': 'sent',
    '3': 'delivered',
    '4': 'signed',
    '5': 'completed',
    '6': 'declined',
    '7': 'deleted'
};

function submitAllEsignatures (e) {
    e.preventDefault();
    var $form = $(e.target.form);
    if ($form.valid()) {
        showLoader();
        $('#fill-all-emails').addClass('hidden')
        var signers = [];
        var rows = $form.find('.signer-row');
        rows.each(function (index, el) {
            var $el = $(el);
            var rowId = $el.find('#row-id').val();
            var signer = {
                Id: $el.find('#signer-id-' + rowId).val(),
                SignatureStatus: $el.find('#signer-status-' + rowId).val(),
                StatusLastUpdateTime: $el.find('#signer-update-' + rowId).val(),
                Role: $el.find('#signer-role-' + rowId).val(),
                CustomerId: $el.find('#signer-customer-id-' + rowId).val(),
                Email: $el.find('#signer-email-' + rowId).val()
            };
            signers.push(signer);
        });
        $.ajax({
            url: esignUrl,
            method: 'POST',
            contentType: 'application/json',
            dataType: 'json',
            data: esignatureModel(signers)
        }).done(function (data) {
            if (!data.isError) {
                rows.each(function (index, el) {
                    var $el = $(el);
                    var rowId = $el.find('#row-id').val();
                    var id = $el.find('#signer-customer-id-' + rowId).val();
                    var signer = data.Signers.filter(function (item) {
                        return item.CustomerId === id;
                    })[0];
                    $el.find('#signer-id-' + rowId).val(signer.Id);
                    $el.find('#signer-status-' + rowId).val(signer.SignatureStatus);
                    $el.find('#signer-update-' + rowId).val(signer.StatusLastUpdateTime);
                    $el.find('.signer-status-hold').removeClass().addClass('.signer-status-hold ' + statusMap[signer.SignatureStatus]);
                    var formated = (signer.StatusLastUpdateTime.getMonth() + 1) + '/' + signer.StatusLastUpdateTime.getDate() + '/' + signer.StatusLastUpdateTime.getFullYear() + ' ' + signer.StatusLastUpdateTime.getHours() + ':' + signer.StatusLastUpdateTime.getMinutes() + ' ' + (now.getHours() > 11 ? 'PM' : 'AM');
                    $el.find('.signature-date-hold').text(formated);
                    if (signer.SignatureStatus === 4 || signer.SignatureStatus === 5) {
                        $el.find('.signature-header').text(translations['ContractSigned']);
                        $el.find('#signer-btn-' + rowId).addClass('hidden');
                    }
                    if (signer.SignatureStatus === 2 || signer.SignatureStatus === 3) {
                        $el.find('.signature-header').text(translations['WaitingSignature']);
                        $el.find('#signer-btn-' + rowId).text(translations['ResendInvite']);
                    }
                    if (signer.SignatureStatus === 0 || signer.SignatureStatus === 1) {
                        var msg = $row.find('#signer-role-' + rowId).val().toLowerCase() === 'additionalapplicant' ?
                            translations['InviteSentWhenSigns'].split('{0}').join(translations['Coborrower']) :
                            translations['InviteSentWhenSigns'].split('{0}').join(translations['Borrower'])
                        $el.find('.signature-header').text(msg);
                        $el.find('#signer-btn-' + rowId).text(translations['UpdateEmail']);
                    }
                });

                $form.find('.signer-status-hold').removeClass('hidden');
                $form.find('#submit-digital').html(translations['CancelDigitalSignature']);
            } else {
                console.log(data);
                dynamicAlertModal({
                    message: data.message,
                    confirmBtnText: translations['Proceed']
                });
            }
        }).fail(function (xhr, status, result) {
            console.log(result);
        }).always(function () {
            hideLoader();
        });
    } else {
        $('#fill-all-emails').removeClass('hidden');
    }
}