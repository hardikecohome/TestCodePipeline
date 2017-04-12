﻿module.exports('financial-functions',
    function() {
        var tax = function(data) {
            return data.equipmentSum * data.tax;
        };

        var totalPrice = function(data) {
            var t = tax(data);
            var equipmentSum = data.equipmentSum;

            return equipmentSum + t;
        };

        var totalAmountFinanced = function(data) {
            var tPrice = totalPrice(data);
            var adminFee = data.adminFee;
            var downPayment = data.downPayment;

            return tPrice + adminFee - downPayment;
        };

        var monthlyPayment = function(data) {
            var tAmountFinanced = totalAmountFinanced(data);
            var amortizationTerm = data.amortTerm;
            var customerRate = data.customerRate;

            return tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0);
        };

        var totalMonthlyPayments = function(data) {
            var mPayment = monthlyPayment(data);
            var loanTerm = data.loanTerm;

            return mPayment * loanTerm;
        };

        var residualBalance = function(data) {
            var amortizationTerm = data.amortTerm;
            var loanTerm = data.loanTerm;
            var customerRate = data.customerRate;
            var mPayment = monthlyPayment(data);

            var rbalance = 0;
            if (loanTerm !== amortizationTerm) {
                rbalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, mPayment, 0) *
                    (1 + customerRate / 100 / 12);
            }

            return rbalance;
        };

        var totalObligation = function(data) {
            var tMonthlyPayments = totalMonthlyPayments(data);
            var rBalance = residualBalance(data);

            return tMonthlyPayments + rBalance;
        };

        var totalBorrowingCost = function(data) {
            var tObligation = totalObligation(data);
            var tAmountFinanced = totalAmountFinanced(data);

            return tObligation - tAmountFinanced;
        };

        return {
            tax: tax,
            totalPrice: totalPrice,
            totalObligation: totalObligation,
            residualBalance: residualBalance,
            totalMonthlyPayments: totalMonthlyPayments,
            monthlyPayment: monthlyPayment,
            totalAmountFinanced: totalAmountFinanced,
            totalBorrowingCost: totalBorrowingCost,
        };
    });