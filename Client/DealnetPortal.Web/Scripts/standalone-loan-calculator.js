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
    var adminFee = calculationState.adminFee;
    if (isNaN(adminFee) || adminFee < 0) {
        adminFee = 0;
    }
    var downPayment = calculationState.downPayment;
    if (isNaN(downPayment) || downPayment < 0) {
        downPayment = 0;
    }
    var totalAmountFinanced = totalCashPrice + adminFee - downPayment;
    calculationState.totalAmountFinanced = totalAmountFinanced;
    if (isNaN(calculationState.loanTerm) || calculationState.loanTerm <= 0 || isNaN(calculationState.amortizationTerm) || calculationState.amortizationTerm <= 0 || isNaN(calculationState.customerRate) || calculationState.customerRate <= 0) { return; }
    var totalMonthlyPayment = totalAmountFinanced * pmt(calculationState.customerRate / 100 / 12, calculationState.amortizationTerm, -1, 0, 0);
    calculationState.totalMonthlyPayment = totalMonthlyPayment;
    var totalAllMonthlyPayments = totalMonthlyPayment * calculationState.loanTerm;
    calculationState.totalAllMonthlyPayments = totalAllMonthlyPayments;
    var residualBalance = 0;
    if (calculationState.loanTerm !== calculationState.amortizationTerm) {
        residualBalance = -pv(calculationState.customerRate / 100 / 12, calculationState.amortizationTerm - calculationState.loanTerm, totalMonthlyPayment, 0) * (1 + calculationState.customerRate / 100 / 12);
    }
    calculationState.residualBalance = residualBalance;
    var totalObligation = totalAllMonthlyPayments + residualBalance;
    calculationState.totalObligation = totalObligation;
    var totalBorrowingCost = totalObligation - totalAmountFinanced;
    calculationState.totalBorrowingCost = totalBorrowingCost;
}