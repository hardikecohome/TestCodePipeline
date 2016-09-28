$(document).ready(function () {
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
    var initPaymentType = $("#payment-type").find(":selected").val();
    managePaymentElements(initPaymentType);
    $("#payment-type").change(function () {
        managePaymentElements($(this).find(":selected").val());
    });
});
function managePaymentElements(paymentType) {
    switch (paymentType) {
        case '0':
            $(".pap-payment").hide();
            $(".enbridge-payment").show();
            break;
        case '1':
            $(".enbridge-payment").hide();
            $(".pap-payment").show();
            break;
    }
}
function setValidationRelation(elem1, elem2) {
    elem1.change(function () {
        if ($(this).val()) {
            elem2.rules("remove", "required");
            elem2.removeClass('input-validation-error');
            elem2.next('.text-danger').empty();
        } else {
            elem2.rules("add", "required");
        }
    });
}