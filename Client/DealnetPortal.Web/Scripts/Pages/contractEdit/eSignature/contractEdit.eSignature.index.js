﻿module.exports('contract-edit.eSignature', function (require) {

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
                        $('#signer-form .signer-status-hold').removeClass().addClass('signer-status-hold');
                        $('#signer-form #submit-digital').text(translations['SendInvites']);
                        $('#signature-status').val('');
                        $('#type-reminder').removeClass('hidden');
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
                data: esignatureModel(signers)
            }).done(function (data) {
            }).fail(function (xhr, status, result) {
                console.log(result);
            }).always(function () {
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
                        var id = parseInt($el.find('#signer-customer-id-' + rowId).val());
                        var signer = data.Signers.filter(function (item) {
                            return isNaN(id) ? item.CustomerId === null : item.CustomerId === id;
                        })[0];
                        $el.find('#signer-id-' + rowId).val(signer.Id);
                        $el.find('#signer-status-' + rowId).val(signer.SignatureStatus);
                        var updateTime = new Date(parseInt(signer.StatusLastUpdateTime.substr(6)));
                        $el.find('#signer-update-' + rowId).val(updateTime);
                        $el.find('.signer-status-hold').removeClass().addClass('col-md-5 signer-status-hold ' + statusMap[signer.SignatureStatus]);
                        var formated = (updateTime.getMonth() + 1) + '/' + updateTime.getDate() + '/' + updateTime.getFullYear() + ' ' + updateTime.getHours() + ':' + updateTime.getMinutes() + ' ' + (updateTime.getHours() > 11 ? 'PM' : 'AM');
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
                            var msg = $el.find('#signer-role-' + rowId).val().toLowerCase() === 'additionalapplicant' ?
                                translations['InviteSentWhenSigns'].split('{0}').join(translations['Coborrower']) :
                                translations['InviteSentWhenSigns'].split('{0}').join(translations['Borrower'])
                            $el.find('.signature-header').text(msg);
                            $el.find('#signer-btn-' + rowId).text(translations['UpdateEmail']);
                        }
                    });

                    $form.find('.signer-status-hold').removeClass('hidden');
                    $form.find('#type-reminder').addClass('hidden');
                    $form.find('#submit-digital').html(translations['CancelDigitalSignature']);
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

    var init = function () {
        $('[id^="signer-btn-"]').on('click', submitOneSignature);
        $('#submit-digital').on('click', submitDigital);
    };

    return {
        init: init
    };
});