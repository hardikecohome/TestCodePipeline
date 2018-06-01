$(document).ready(function () {
    $('.credit-funding-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', '1');
	});
	$('.funding-contact').on('click', function () {
		$('body').removeClass('open-menu menu-animated');
		$('.dealnet-sidebar').removeClass('in');
		sendEmailModel('', '2');
	});
    $('.dealer-support-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', '0');
    });
    $('.customer-service-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', '6');
    });
});

function sendEmailModel (rowTransactionId, supportType) {
    if (supportType == null) {
        supportType = '6';
    }
    if (rowTransactionId == null) {
        rowTransactionId = '';
    }
    $('#emailSupport').removeAttr("disabled", "disabled");
    $('#emailTransactionId').val(rowTransactionId);
    $('#emailDealerName').val(DealerName).removeAttr('type');
    $('#emailSubDealerName').val(DealerName).removeAttr('type');
    $("#SupportType option").each(function () {
        if ($(this).val() == supportType) {
            $(this).attr('selected', 'selected');
        }
        else {
            $(this).removeAttr('selected');
        }
    });
    var alertModal = $('#emailModal');
    alertModal.find('#emailTransactionId').text(rowTransactionId);
    alertModal.validate();
    alertModal.modal('show');

    $('#yourNameCB').on('click', function () {
        if ($('#yourNameCB').prop('checked')) {
            $('#yourNameTxt').removeAttr("disabled").removeClass('controlDisabledGrey');
            $('#IsPreferedContactPerson').addClass('mandatory-field');
        }
        else {
            $('#yourNameTxt').addClass('controlDisabledGrey').attr("disabled", "disabled");
            $('#IsPreferedContactPerson').removeClass('mandatory-field');
        }
    });
    $('#CommunicationPreffered').on('change', function () {
        var bestWay = $('#CommunicationPreffered').val();
        if (bestWay == 1) {
            $('#BestWayPhoneDiv').addClass('hidden');
            $('#BestWayEmailDiv').removeClass('hidden');
        }
        else {
            $('#BestWayEmailDiv').addClass('hidden');
            $('#BestWayPhoneDiv').removeClass('hidden');
        }

    });
    $('#emailModal').on('hide.bs.modal', function () {
        var form = $('#Help-Pop-Up');
        form.validate().resetForm();
        form.find('.text-danger').empty();
        $('#BestWayEmailDiv').addClass('hidden');
        $('#BestWayPhoneDiv').removeClass('hidden');
        $('#yourNameTxt').addClass('controlDisabledGrey').attr("disabled", "disabled");
        $('#IsPreferedContactPerson').removeClass('mandatory-field');
        $('#sent-success').addClass('hidden');
    });

}

function sendEmailToSupport(url, form, data) {
    form.validate();
    if (!form.valid()) {
        return false;
    };
    $('#emailSupport').attr("disabled", "disabled");
    $.ajax({
        type: "POST",
        url: url,
        data: form.serialize(),
        success: function (json) {
            $('#sent-success').removeClass('hidden');
            $('#emailSupport').removeAttr("disabled", "disabled");
        },
        error: function (xhr, status, p3) {
            $('#emailSupport').removeAttr("disabled", "disabled");
        }
    });

}