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
    if (isNaN(equipmentCashPrice) || equipmentCashPrice <= 0) {
        return;
    }
    var hst = 0;
    //Matt's request to map new contract
    //var hst = taxRate / 100 * equipmentCashPrice;
    hstLabel.text(formatNumber(hst));
    var totalCashPrice = equipmentCashPrice + hst;
    totalCashPriceLabel.text(formatNumber(totalCashPrice));
    var isFeePaidByCustomer = $('#fee-paid-by-customer').val().toLowerCase() == 'true';
    var adminFee = parseFloat($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0 || !isFeePaidByCustomer) {
        adminFee = 0;
    }
    var downPayment = parseFloat($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice - downPayment + adminFee;
    totalAmountFinancedLabel.text(formatNumber(totalAmountFinanced));

    var loanTerm = parseInt($("#loan-term").val());
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = parseFloat($("#customer-rate").val());
    if (isNaN(loanTerm) || loanTerm <= 0 || isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate < 0) {
        return;
    }
    var totalMonthlyPayment = customerRate > 0 ? (Math.round(totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0) * 100) / 100) : (totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0));
    isCalculationValid = totalMonthlyPayment > 0;
    loanTotalMonthlyPaymentLabel.text(formatNumber(totalMonthlyPayment));

    var totalAllMonthlyPayments = totalMonthlyPayment * loanTerm;
    loanTotalAllMonthlyPaymentsLabel.text(formatNumber(totalAllMonthlyPayments));

    var residualBalance = 0;
    if (loanTerm !== amortizationTerm) {
        residualBalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, totalMonthlyPayment, 0);
    }
    residualBalanceLabel.text(formatNumber(residualBalance));

    var totalObligation = totalAllMonthlyPayments + residualBalance;
    totalObligationLabel.text(formatNumber(totalObligation));

    var totalBorrowingCost = Math.abs(totalObligation - totalAmountFinanced + adminFee);
    totalBorrowingCostLabel.text(formatNumber(totalBorrowingCost));
}

function checkCalculationValidity(inputCashPrice, inputTaxRate) {
    var equipmentCashPrice = typeof inputCashPrice !== 'undefined' && inputCashPrice !== null ? inputCashPrice : parseFloat($("#equipment-cash-price").text());
    if (isNaN(equipmentCashPrice) || equipmentCashPrice <= 0) {
        return false;
    }
    var testTaxRate = typeof inputTaxRate !== 'undefined' && inputTaxRate !== null ? inputTaxRate : taxRate;
    var hst = testTaxRate / 100 * equipmentCashPrice;
    var totalCashPrice = equipmentCashPrice + hst;
    var adminFee = parseFloat($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = parseFloat($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice - downPayment;
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = parseFloat($("#customer-rate").val());
    if (isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate < 0) {
        return false;
    }
    var totalMonthlyPayment = Math.round(totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0) * 100) / 100;
    return totalMonthlyPayment > 0;
}

function calculateClarityTotalCashPrice() {
    var clarityCalcualtions = module.require('financial-functions.clarity');
    var hstLabel = $("#hst");
    var totalAmountFinancedLabel = $("#displayTotalAmount");
    var totalMonthlyPaymentsLabel = $("#displayAllMonthly");
    var displayDealerCost = $("#displayDealerCost");
    var balanceOwning = $("#displayBalanceOwning");
    var totalObligationLabel = $("#displayTotalObl");
    var borrowerCost = $("#displayCostOfBorrow");
    var yourCost = $("#displayYourCost");
    var loantTermDisplay = $("#displayLoanTerm");
    var amortTermDisplay = $("#displayAmortTem");
    var displayCustRate = $('#displayCustRate');
    var displayMonthlyCostNoTax = $('#totalMonthlyCostNoTax');
    var displayTotalHst = $('#total-hst');
    var displayTotalMonthlyCostTax = $('#totalMonthlyCostTax');
    var displaytotalMonthlyCostTaxDP = $('#totalMonthlyCostTaxDP')

    hstLabel.text('-');
    totalAmountFinancedLabel.text('-');
    totalMonthlyPaymentsLabel.text('-');
    totalObligationLabel.text('-');
    borrowerCost.text('-');
    yourCost.text('-');
    balanceOwning.text('-');
    displayMonthlyCostNoTax.text('-');
    displayTotalHst.text('-');
    displayTotalMonthlyCostTax.text('-');
    displaytotalMonthlyCostTaxDP.text('-');

    var equipmentCashPrice = parseFloat($("#equipment-cash-price").text());
    var packageCashPrice = parseFloat($("#package-cash-price").text());
    var hst = taxRate;
    var loanTerm = parseInt($("#loan-term").val());
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = parseFloat($("#customer-rate").val());
    var adminFee = parseFloat($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = parseFloat($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }

    var data = {
        tax: hst,
        equipmentSum: equipmentCashPrice,
        packagesSum: packageCashPrice,
        LoanTerm: loanTerm,
        AmortizationTerm: amortizationTerm,
        CustomerRate: customerRate,
        AdminFee: adminFee,
        downPayment: downPayment,
        DealerCost: 0
    }

    hstLabel.text(formatNumber(clarityCalcualtions.tax(data)));
    totalAmountFinancedLabel.text(formatNumber(clarityCalcualtions.totalAmountFinanced(data)));
    totalMonthlyPaymentsLabel.text(formatNumber(clarityCalcualtions.totalMonthlyPayments(data)));
    totalObligationLabel.text(formatNumber(clarityCalcualtions.totalObligation(data)));
    borrowerCost.text(formatNumber(clarityCalcualtions.totalBorrowingCost(data)));
    balanceOwning.text(formatNumber(clarityCalcualtions.residualBalance(data)));
    yourCost.text(formatNumber(clarityCalcualtions.yourCost(data)));
    displayCustRate.text(customerRate + ' %');
    loantTermDisplay.text(loanTerm + ' months');
    amortTermDisplay.text(amortizationTerm + ' months');
    amortTermDisplay.text(amortizationTerm + ' months');
    displayDealerCost.text(0 + ' %');
    displayMonthlyCostNoTax.text(formatNumber(clarityCalcualtions.totalMCONoTax(data)));
    displayTotalHst.text(formatNumber(clarityCalcualtions.tax(data)));
    displaytotalMonthlyCostTaxDP.text(formatNumber(clarityCalcualtions.totalMonthlyPaymentsLessDownPayment(data)));

    displayTotalMonthlyCostTax.text(formatNumber(clarityCalcualtions.totalMCOLessDownPaymentNoTax(data)));

}