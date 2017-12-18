$(document).ready(function () {
    $('.credit-funding-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', 'creditFunding');
    });
    $('.dealer-support-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', 'dealerSupport');
    });
    $('.customer-service-contact').on('click', function () {
        $('body').removeClass('open-menu menu-animated');
        $('.dealnet-sidebar').removeClass('in');
        sendEmailModel('', 'customerService');
    });
});

function clearEmailForm () {
    //$('#emailDealerName').text("");
    $('#yourNameCB').prop('checked', false);
    $('#yourNameTxt').attr("disabled", "disabled");
    $('#yourNameTxt').val("");
    //$('#emailSubDealerName').text("");
    $('#emailTransactionId').val("");
    $('#SupportType').val("Other");
    $('#CommunicationPreffered').val("Phone");
    $('#emailComment').val("");
    $('#emailPhone').prop('checked', false);
    $('#emailSameEmail').prop('checked', false);
    $('#emailAlternativeEmail').prop('checked', false);
    $('#emailAlternativeEmailAddress').val("");
    $('#alternativeEmailDiv').addClass('hidden');
    $('#emailSupport').removeAttr("disabled", "disabled");
    $('#yourNameTxtValidation').addClass('hidden');
    $('#emailCommentValidation').addClass('hidden');
    $('#emailAlternativeEmailAddressValidation').addClass('hidden');
}
function CommunicationPreffered () {
    $('#ContactDetails').text($('#CommunicationPreffered').find("option:selected").text());
    $('#emailAlternativeEmailAddress').val("");
    $('#emailAlternativeEmailAddressValidation').addClass('hidden');
}
function sendEmailModel (rowTransactionId, supportType) {
    if (supportType == null) {
        supportType = 'Other';
    }
    var alertModal = $('#emailModal');

    clearEmailForm();
    $('#emailTransactionId').val(rowTransactionId);
    //alertModal.find('#emailTransactionId').text(rowTransactionId);
    $('#SupportType').val(supportType);
    alertModal.modal('show');
}
function yourNameCBclick () {
    if ($('#yourNameCB').prop('checked')) {
        $('#yourNameTxt').removeAttr("disabled");
    }
    else {
        $('#yourNameTxt').attr("disabled", "disabled");
        $('#yourNameTxt').val("");
        $('#yourNameTxtValidation').addClass('hidden');
    }
}

//function emailAlternativeEmailclick () {
//    if ($('#emailAlternativeEmail').prop('checked')) {
//        $('#emailAlternativeEmailAddress').removeClass('hidden');
//    }
//    else {
//        $('#emailAlternativeEmailAddress').addClass('hidden');

//    }
//}
//$.validator.addMethod("requiredIfChecked", function (val, ele, arg) {
//	if ($("#startClientFromWeb").is(":checked") && ($.trim(val) == '')) { return false; }
//	return true;
//}, "This field is required if startClientFromWeb is checked...");

function validateEmail (Email) {
    var pattern = /^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
    return $.trim(Email).match(pattern) ? true : false;
}
function validatePhone (Phone) {
    if (Phone.length != 10 || isNaN(Phone)) {
        return false;
    }
    else {
        return true;
    }
}
function sendEmailToSupport () {
    //check validation
    if ($("#yourNameCB").is(":checked") && ($.trim($('#yourNameTxt').val()) == '')) {
        $('#yourNameTxtValidation').removeClass('hidden');
        return false;
    }
    else {
        $('#yourNameTxtValidation').addClass('hidden');
    }
    if ($.trim($('#emailComment').val()) == '') {
        $('#emailCommentValidation').removeClass('hidden');
        return false;
    }
    else {
        $('#emailCommentValidation').addClass('hidden');
    }

    if ($('#CommunicationPreffered').val() == "Email" && (($.trim($('#emailAlternativeEmailAddress').val()) == '') || !(validateEmail($('#emailAlternativeEmailAddress').val().trim())))) {
        $('#emailAlternativeEmailAddressValidation').removeClass('hidden');
        return false;
    }
    else if ($('#CommunicationPreffered').val() == "Phone" && (($.trim($('#emailAlternativeEmailAddress').val()) == '') || !(validatePhone($('#emailAlternativeEmailAddress').val().trim())))) {
        $('#emailAlternativeEmailAddressValidation').removeClass('hidden');
        return false;
    }
    else {
        $('#emailAlternativeEmailAddressValidation').addClass('hidden');
    }

    //disable button
    $('#emailSupport').attr("disabled", "disabled");

    var data = {
        "Id": 0,
        "DealerName": $('#emailDealerName').text(),
        "YourName": $('#yourNameCB').prop('checked') ? $('#yourNameTxt').val() : $('#emailSubDealerName').text(),
        "LoanNumber": $('#emailTransactionId').val(),
        "SupportType": $('#SupportType').val(),
        "HelpRequested": $('#emailComment').val(),
        "BestWay": $('#CommunicationPreffered').val(),
        "ContactDetails": $('#emailAlternativeEmailAddress').val()
    };
    $.ajax({
        cache: false,
        method: "POST",
        url: SupportUrl,
        data: data,
        success: function (json) {
            $('#emailModal').modal('toggle');
            clearEmailForm();
            //enable button

        }
    });

}