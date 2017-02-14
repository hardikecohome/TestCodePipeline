function calculateAndAssign(calculationState) {
    if (!calculationState.equipment) {
        return;
    }
    var equipmentCashPrice;
    $.each(calculationState.equipment, function (index, value) {
        var numberValue = parseFloat(value.cost);
        if (!isNaN(numberValue)) {
            if (!equipmentCashPrice) {
                equipmentCashPrice = 0;
            }
            equipmentCashPrice += numberValue;
        }
    });
    if (!equipmentCashPrice || equipmentCashPrice <= 0) {
        return;
    }
    calculationState.equipmentCashPrice = equipmentCashPrice;
    var hst = taxRate / 100 * equipmentCashPrice;
    calculationState.hst = hst;
    var totalCashPrice = equipmentCashPrice + hst;
    calculationState.totalCashPrice = totalCashPrice;
    var adminFee = parseFloat(calculationState.adminFee);
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = parseFloat(calculationState.downPayment);
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    calculationState.totalAmountFinanced = totalAmountFinanced;
    var loanTerm = parseInt(calculationState.loanTerm);
    var amortizationTerm = parseInt(calculationState.amortizationTerm);
    var customerRate = parseFloat(calculationState.customerRate);
    if (isNaN(loanTerm) || loanTerm <= 0 || isNaN(amortizationTerm) || amortizationTerm <= 0 || isNaN(customerRate) || customerRate < 0) { return; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
    calculationState.totalMonthlyPayment = totalMonthlyPayment;
    var totalAllMonthlyPayments = totalMonthlyPayment * loanTerm;
    calculationState.totalAllMonthlyPayments = totalAllMonthlyPayments;
    var residualBalance = 0;
    if (loanTerm !== amortizationTerm) {
        residualBalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, totalMonthlyPayment, 0) * (1 + customerRate / 100 / 12);
    }
    calculationState.residualBalance = residualBalance;
    var totalObligation = totalAllMonthlyPayments + residualBalance;
    calculationState.totalObligation = totalObligation;
    var totalBorrowingCost = totalObligation - totalAmountFinanced;
    calculationState.totalBorrowingCost = totalBorrowingCost;
}