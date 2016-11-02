function calculateLoanValues() {
    var equipmentCashPrice = parseFloat($("#equipment-cash-price").text());
    var hst = taxRate / 100 * equipmentCashPrice;
    $("#hst").text(hst);
    var totalCashPrice = equipmentCashPrice + hst;
    $("#total-cash-price").text(totalCashPrice);
    var adminFee = parseFloat($("#admin-fee").val());
    var downPayment = parseFloat($("#down-payment").val());
    if (isNaN(adminFee) || isNaN(downPayment)){ return; }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    $("#total-amount-financed").text(totalAmountFinanced);
    var loanTerm = parseInt($("#requested-term").val());
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = parseFloat($("#customer-rate").val());
    if (isNaN(loanTerm) || isNaN(amortizationTerm) || isNaN(customerRate)) { return; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
    $("#loan-total-monthly-payment").text(totalMonthlyPayment.toFixed(2));
    var totalAllMonthlyPayments = totalMonthlyPayment * loanTerm;
    $("#loan-total-all-monthly-payments").text(totalAllMonthlyPayments.toFixed(2));
    var residualBalance = 0;
    if (loanTerm !== amortizationTerm) {
        residualBalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, totalMonthlyPayment, 0) * (1 + customerRate / 100 / 12);
    }
    $("#residual-balance").text(residualBalance.toFixed(2));
    var totalObligation = totalAllMonthlyPayments + residualBalance;
    $("#total-obligation").text(totalObligation.toFixed(2));
    var totalBorrowingCost = totalObligation - totalAmountFinanced;
    $("#total-borrowing-cost").text(totalBorrowingCost.toFixed(2));
}

function pmt(rate_per_period, number_of_payments, present_value, future_value, type) {
    if (rate_per_period != 0.0) {
        var q = Math.pow(1 + rate_per_period, number_of_payments);
        return -(rate_per_period * (future_value + (q * present_value))) / ((-1 + q) * (1 + rate_per_period * (type)));

    } else if (number_of_payments != 0.0) {
        return -(future_value + present_value) / number_of_payments;
    }

    return 0;
}

function pv(rate, nper, pmt, fv)

{

    nper = parseFloat(nper);

    pmt = parseFloat(pmt);

    fv = parseFloat(fv);

    rate = parseFloat(rate);;

    if (( nper == 0 )) {

        return(0);

    }

    if ( rate == 0 )

    {

        pv_value = -(fv + (pmt * nper));

    }

    else

    {

        x = Math.pow(1 + rate, -nper);

        y = Math.pow(1 + rate, nper);

        pv_value = - ( x * ( fv * rate - pmt + y * pmt )) / rate;

    }

    pv_value = conv_number(pv_value,2);

    return (pv_value);

}

function conv_number(expr, decplaces)

{
    var str = "" + Math.round(eval(expr) * Math.pow(10,decplaces));

    while (str.length <= decplaces) {

        str = "0" + str;

    }

    var decpoint = str.length - decplaces;

    return (str.substring(0,decpoint) + "." + str.substring(decpoint,str.length));

}