configInitialized
    .then(function () {
    $(".home-phone").each(function () {
        $(this).rules("add", "required");
    });
    $(".cell-phone").each(function () {
        $(this).rules("add", "required");
    });
    $("#enbridge-gas-distribution-account").rules("add", "required");
    $("#meter-number").rules("add", "required");

    $('.mandatory-phones').each(function() {
        var homePhone = $(this).find('.home-phone');
        var cellPhone = $(this).find('.cell-phone');
        if (homePhone.length && cellPhone.length) {
            setValidationRelation(homePhone, cellPhone);
            setValidationRelation(cellPhone, homePhone);
        }
    });
    setValidationRelation($("#enbridge-gas-distribution-account"), $("#meter-number"));
    setValidationRelation($("#meter-number"), $("#enbridge-gas-distribution-account"));
    $(".home-phone").change();
    $(".cell-phone").change();
    $("#enbridge-gas-distribution-account").change();
    $("#meter-number").change();
    var initPaymentType = $("#payment-type").find(":selected").val();
    managePaymentElements(initPaymentType);
    $("#payment-type").change(function () {
        managePaymentElements($(this).find(":selected").val());
    });

    $('form').on('submit', function(e) {
        if (!$('form').valid()) {
            e.preventDefault();
        }

        var isValid = $('#agreement-checkbox').prop('checked');

        if ($('#additional-agreement-checkbox').length) {
            isValid = $('#agreement-checkbox').prop('checked') && $('#additional-agreement-checkbox').prop('checked');
        }

        if (!isValid) {
            e.preventDefault();
            $('#proceed-error-message').show();
        } else {
            $('#proceed-error-message').hide();
        }
    });
});

function managePaymentElements(paymentType) {
    switch (paymentType) {
        case '0':
            $(".pap-payment").hide();
            $(".pap-payment").find('input, select').each(function () {
                $(this).prop("disabled", true);
            });
            $(".enbridge-payment").show();
            $(".enbridge-payment").find('input, select').each(function () {
                $(this).prop("disabled", false);
            });
            break;
        case '1':
            $(".enbridge-payment").hide();
            $(".enbridge-payment").find('input, select').each(function () {
                $(this).prop("disabled", true);
            });
            $(".pap-payment").show();
            $(".pap-payment").find('input, select').each(function () {
                $(this).prop("disabled", false);
            });
            break;
    }
}