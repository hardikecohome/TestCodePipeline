﻿function calculateLoanValues() {
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
    var equipmentCashPrice = Globalize.parseNumber($("#equipment-cash-price").text());
    if (isNaN(equipmentCashPrice) || equipmentCashPrice <= 0) { return; }
    var hst = taxRate / 100 * equipmentCashPrice;
    hstLabel.text(formatNumber(hst));
    var totalCashPrice = equipmentCashPrice + hst;
    totalCashPriceLabel.text(formatNumber(totalCashPrice));
    var adminFee = Globalize.parseNumber($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = Globalize.parseNumber($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    totalAmountFinancedLabel.text(formatNumber(totalAmountFinanced));
    var loanTerm = parseInt($("#loan-term").val());
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = Globalize.parseNumber($("#customer-rate").val());
    if (isNaN(loanTerm) || loanTerm <= 0 || isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate < 0) { return; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
    isCalculationValid = totalMonthlyPayment > 0;
    loanTotalMonthlyPaymentLabel.text(formatNumber(totalMonthlyPayment));
    var totalAllMonthlyPayments = totalMonthlyPayment * loanTerm;
    loanTotalAllMonthlyPaymentsLabel.text(formatNumber(totalAllMonthlyPayments));
    var residualBalance = 0;
    if (loanTerm !== amortizationTerm) {
        residualBalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, totalMonthlyPayment, 0) * (1 + customerRate / 100 / 12);
    }
    residualBalanceLabel.text(formatNumber(residualBalance));
    var totalObligation = totalAllMonthlyPayments + residualBalance;
    totalObligationLabel.text(formatNumber(totalObligation));
    var totalBorrowingCost = Math.abs(totalObligation - totalAmountFinanced);
    totalBorrowingCostLabel.text(formatNumber(totalBorrowingCost));
}

function checkCalculationValidity(inputCashPrice, inputTaxRate) {
    var equipmentCashPrice = typeof inputCashPrice !== 'undefined' && inputCashPrice !== null ? inputCashPrice : Globalize.parseNumber($("#equipment-cash-price").text());
    if (isNaN(equipmentCashPrice) || equipmentCashPrice <= 0) { return false; }
    var testTaxRate = typeof inputTaxRate !== 'undefined' && inputTaxRate !== null ? inputTaxRate : taxRate;
    var hst = testTaxRate / 100 * equipmentCashPrice;
    var totalCashPrice = equipmentCashPrice + hst;
    var adminFee = Globalize.parseNumber($("#admin-fee").val());
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = Globalize.parseNumber($("#down-payment").val());
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    var amortizationTerm = parseInt($("#amortization-term").val());
    var customerRate = Globalize.parseNumber($("#customer-rate").val());
    if (isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate < 0) { return false; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
    return totalMonthlyPayment > 0;
}