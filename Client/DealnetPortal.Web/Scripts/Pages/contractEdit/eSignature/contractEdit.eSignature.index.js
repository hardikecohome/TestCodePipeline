module.exports('contract-edit.eSignature', function (require) {
  
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
        if (status === 'sent' || status === 'delivered')
            cancelSignatures(e);
        else
            submitAllEsignatures(e, status);
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
                        var form = $('#signer-form');
                        form.find('input').prop('disabled', false);
                        form.find('.signer-status-hold').removeClass().addClass('signer-status-hold');
                        form.find('#submit-digital').text(translations['SendInvites']);
                        $('#signature-status').val('');
                        $('#type-reminder').removeClass('hidden');
                        $('.signature-notification').text(translations['DigitalInvitesWillBeSentToEmails']);
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
        var rowId = $row.find('#row-id').val();
        var email = $row.find('#signer-email-' + rowId);
        if (email.valid()) {
            showLoader();

            var signers = [{
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
                cache: false,
                data: esignatureModel(signers)
            }).done(function (data) {
                $row.find('.signer-btn-hold .icon').removeClass('hidden');
            }).fail(function (xhr, status, result) {
                console.log(result);
            }).always(function () {
                hideLoader();
            });
        }
    }

    function submitAllEsignatures (e, previousStatus) {
        e.preventDefault();
        var $form = $(e.target.form);
        if ($form.valid()) {
            showLoader();
            $('#fill-all-emails').addClass('hidden');
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
                cache: false,
                data: esignatureModel(signers)
            }).done(function (data) {
                if (!data.isError) {
                    $('#signature-status').val(statusMap[data.Status]);
                    rows.each(function (index, el) {
                        var $el = $(el);
                        var rowId = $el.find('#row-id').val();
                        if (previousStatus === 'declined' && $('#signer-email-' + rowId).is(':disabled')) {
                            $('#signer-email-' + rowId).removeAttr('disabled');
                        }

                        var id = parseInt($el.find('#signer-customer-id-' + rowId).val());
                        var signer = data.Signers.filter(function (item) {
                            return isNaN(id) ? item.CustomerId === null : item.CustomerId === id;
                        })[0];
                        $el.find('#signer-id-' + rowId).val(signer.Id);
                        $el.find('#signer-status-' + rowId).val(signer.SignatureStatus);
                        var updateTime = new Date(parseInt(signer.StatusLastUpdateTime.substr(6)));
                        $el.find('.signer-status-hold').removeClass().addClass('col-md-5 signer-status-hold ' + statusMap[signer.SignatureStatus]);
                        var formated = updateTime.getUTCMonth() + 1 + '/' + updateTime.getUTCDate() + '/' + updateTime.getUTCFullYear() + ' ' + (updateTime.getUTCHours() > 12 ? updateTime.getUTCHours() - 12 : updateTime.getUTCHours()) + ':' + (updateTime.getUTCMinutes() < 10 ? '0' + updateTime.getUTCMinutes() : updateTime.getUTCMinutes()) + ' ' + (updateTime.getUTCHours() > 11 ? 'PM' : 'AM');
                        $el.find('#signer-update-' + rowId).val(formated);
                        $el.find('.signature-date-hold').text(formated);
                        if (signer.SignatureStatus === 4 || signer.SignatureStatus === 5) {
                            $el.find('.signature-header').text(translations['ContractSigned']);
                            $el.find('#signer-btn-' + rowId).addClass('hidden');
                            $el.find('.icon-front').replaceWith($form.find('.icons .icon-success').clone());
                        }
                        if (signer.SignatureStatus === 2 || signer.SignatureStatus === 3) {
                            $el.find('.signature-header').text(translations['WaitingSignature']);
                            $el.find('#signer-btn-' + rowId).text(translations['ResendInvite']);
                            $el.find('.icon-front').replaceWith($form.find('.icons .icon-waiting').clone());
                        }
                        if (signer.SignatureStatus === 0 || signer.SignatureStatus === 1) {
                            var role = $('#signer-role-' + (rowId - 1)).val().toLowerCase();
                            var msg = role === 'homeowner' || role === 'signer' ?
                                translations['InviteSentWhenSigns'].split('{0}').join(translations['Borrower']) :
                                translations['InviteSentWhenSigns'].split('{0}').join(translations['Coborrower']);
                            $el.find('.signature-header').text(msg);
                            $el.find('#signer-btn-' + rowId).text(translations['UpdateEmail']);
                            $el.find('.icon-front').replaceWith($form.find('.icons .icon-waiting').clone());
                        }
                        if (signer.Comment) {
                            $el.find('.comment-btn').removeClass('hidden');
                            $('#comment-' + rowId + ' text-semibold').text(signer.Comment);
                        } else {
                            $el.find('.comment-btn').addClass('hidden');
                            $('#comment-' + rowId + ' text-semibold').addClass('hidden');
                        }
                    });

                    $form.find('.signature-date').removeClass('hidden');
                    $form.find('.signer-status-hold').removeClass('hidden');
                    $form.find('#type-reminder').addClass('hidden');
                    $form.find('#submit-digital').html(translations['CancelDigitalSignature']);
                    $('.signature-notification').text(translations['InvitesSentWaitingSignatures']);
                    $('#contact-before-resend').addClass('hidden');
                    $('#fill-all-emails').addClass('hidden');
                } else {
                    console.log(data);
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

    function toggleComment (e) {
        var $row = $(e.target).parents('.signer-row');
        var rowId = $row.find('#row-id').val();
        $('#comment-' + rowId).toggleClass('hidden');
    }

    var init = function () {
        $('[id^="signer-btn-"]').on('click', submitOneSignature);
        $('#submit-digital').on('click', submitDigital);

        //work around of IE SVG system
        $('.comment-btn').each(function () {
            var $this = $(this);
            var $row = $this.parents('.signer-row');
            var rowId = $row.find('#row-id').val();
            $this.on('click', function () {
                $('#comment-' + rowId).toggleClass('hidden');
                $row.toggleClass('bordered');
            });
        });
    };

    return {
        init: init
    };
});