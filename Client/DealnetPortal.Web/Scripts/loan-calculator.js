function calculateLoanValues() {
    var hstLabel = $("#hst");
    var totalCashPriceLabel = $("#total-cash-price");
    var totalAmountFinancedLabel = $("#total-amount-financed");
    var loanTotalMonthlyPaymentLabel = $("#loan-total-monthly-payment");
    var loanTotalAllMonthlyPaymentsLabel = $("#loan-total-all-monthly-payments");
    var residualBalanceLabel = $("#residual-balance");
    var totalObligationLabel = $("#total-obligation");
    var totalBorrowingCostLabel = $("#total-borrowing-cost");
    hstLabel.text('-');
    totalCashPriceLabel.text('-');
    totalAmountFinancedLabel.text('-');
    loanTotalMonthlyPaymentLabel.text('-');
    loanTotalAllMonthlyPaymentsLabel.text('-');
    residualBalanceLabel.text('-');
    totalObligationLabel.text('-');
    totalBorrowingCostLabel.text('-');
    //
    var equipmentCashPrice = parseFloat($("#equipment-cash-price").text());
    if (isNaN(equipmentCashPrice) || equipmentCashPrice <= 0) { return; }
    var hst = taxRate / 100 * equipmentCashPrice;
    hstLabel.text(hst.toFixed(2));
    var totalCashPrice = equipmentCashPrice + hst;
    totalCashPriceLabel.text(totalCashPrice.toFixed(2));
    var adminFee = parseFloat($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = parseFloat($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    totalAmountFinancedLabel.text(totalAmountFinanced.toFixed(2));
    var loanTerm = parseInt($("#loan-term").val());
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = parseFloat($("#customer-rate").val());
    if (isNaN(loanTerm) || loanTerm <= 0 || isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate <= 0) { return; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
    isCalculationValid = totalMonthlyPayment > 0;
    loanTotalMonthlyPaymentLabel.text(totalMonthlyPayment.toFixed(2));
    var totalAllMonthlyPayments = totalMonthlyPayment * loanTerm;
    loanTotalAllMonthlyPaymentsLabel.text(totalAllMonthlyPayments.toFixed(2));
    var residualBalance = 0;
    if (loanTerm !== amortizationTerm) {
        residualBalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, totalMonthlyPayment, 0) * (1 + customerRate / 100 / 12);
    }
    residualBalanceLabel.text(residualBalance.toFixed(2));
    var totalObligation = totalAllMonthlyPayments + residualBalance;
    totalObligationLabel.text(totalObligation.toFixed(2));
    var totalBorrowingCost = totalObligation - totalAmountFinanced;
    totalBorrowingCostLabel.text(totalBorrowingCost.toFixed(2));
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