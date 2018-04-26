﻿module.exports('financial-functions',
    function () {
        var tax = function (data) {
            return data.equipmentSum * data.tax / 100;
        };

        var totalRentalPrice = function (data) {
            var t = tax(data);
            var equipmentSum = data.equipmentSum;

            return equipmentSum + t;
        };

        var totalPrice = function (data) {
            //var t = tax(data);
            var equipmentSum = data.equipmentSum;

            return equipmentSum /*+ t*/ ;
        };

        var totalAmountFinanced = function (data) {
            var tPrice = totalPrice(data);
            var downPayment = data.downPayment;
            var includeAdminFee = data.includeAdminFee !== undefined ? data.includeAdminFee : false;
            var withAdminFee = tPrice + data.AdminFee - downPayment;
            var withoutAdminFee = tPrice - downPayment;

            return includeAdminFee ? withAdminFee : withoutAdminFee;
        };

        var yourCost = function (data) {
            var includeAdminFee = data.includeAdminFee !== undefined ? !data.includeAdminFee : false;
            var taf = totalAmountFinanced(data);
            var adjustedDealerCost = +(taf * (data.InterestRateReduction / 100)).toFixed(2);

            var yCost = data.DealerCost * taf / 100;

            return includeAdminFee ? yCost + data.AdminFee + adjustedDealerCost : yCost + adjustedDealerCost;
        }

        var monthlyPayment = function (data) {
            var tAmountFinanced = totalAmountFinanced(data);
            var amortizationTerm = data.AmortizationTerm;
            var customerRate = data.CustomerRate;

            return customerRate > 0 ? (Math.round((tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0)) * 100) / 100) :
                (tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0));
        };

        var totalMonthlyPayments = function (data) {
            var mPayment = monthlyPayment(data);
            var loanTerm = data.LoanTerm;

            return mPayment * loanTerm;
        };

        var residualBalance = function (data) {
            var amortizationTerm = data.AmortizationTerm;
            var loanTerm = data.LoanTerm;
            var customerRate = data.CustomerRate;
            var mPayment = monthlyPayment(data);

            var rbalance = 0;
            if (loanTerm !== amortizationTerm) {
                rbalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, mPayment, 0) *
                    (1 + customerRate / 100 / 12);
            }

            return rbalance;
        };

        var totalObligation = function (data) {
            var tMonthlyPayments = totalMonthlyPayments(data);
            var rBalance = residualBalance(data);
            //         var includeAdminFee = data.includeAdminFee !== undefined ? data.includeAdminFee : false;
            //var adminFee = includeAdminFee ? data.AdminFee : 0;
            return tMonthlyPayments + rBalance /*+ adminFee*/ ;
        };

        var totalBorrowingCost = function (data) {
            var tObligation = totalObligation(data);
            var tAmountFinanced = totalAmountFinanced(data);
            var includeAdminFee = data.includeAdminFee !== undefined ? data.includeAdminFee : false;
            var adminFee = includeAdminFee ? data.AdminFee : 0;

            var borrowingCost = tObligation - tAmountFinanced + adminFee;
            if (borrowingCost < 0)
                borrowingCost = 0;
            return borrowingCost;
        };

        return {
            tax: tax,
            totalPrice: totalPrice,
            totalRentalPrice: totalRentalPrice,
            totalObligation: totalObligation,
            residualBalance: residualBalance,
            totalMonthlyPayments: totalMonthlyPayments,
            monthlyPayment: monthlyPayment,
            totalAmountFinanced: totalAmountFinanced,
            totalBorrowingCost: totalBorrowingCost,
            yourCost: yourCost
        };
    });