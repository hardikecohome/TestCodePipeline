module.exports('financial-functions.clarity',
    function () {

        var clarityPaymentFactor = 0.010257;

        var clarityTax = function (data) {
            return (data.packagesSum + data.equipmentSum) * data.tax / 100;
        };

        var totalMCONoTax = function (data) {
            var equipmentSum = data.equipmentSum;
            var packagesSum = data.packagesSum;

            return equipmentSum + packagesSum;
        }

        var totalMonthlyCostOfOwnership = function (data) {
            var t = clarityTax(data);
            var mcoNoTax = totalMCONoTax(data);

            return mcoNoTax + t;
        };

        var totalClarityAmountFinanced = function (data) {
            var tPrice = totalPriceOfEquipment(data);
            var downPayment = data.downPayment;
            //var amortizationTerm = data.AmortizationTerm;
            //var customerRate = data.CustomerRate;

            var res = tPrice - downPayment;

            //var res = -pv(customerRate / 100 / 12, amortizationTerm, tPrice, 0);

            return res;
        }

        var totalClarityMonthlyPayments = function (data) {
            var mPayment = totalClarityMonthlyPaymentsLessDownPayment(data);
            var loanTerm = data.LoanTerm;

            return mPayment.toFixed(2) * loanTerm;
        };

        var clarityResidualBalance = function (data) {
            var amortizationTerm = data.AmortizationTerm;
            var loanTerm = data.LoanTerm;
            var customerRate = data.CustomerRate;
            var mPayment = formatNumber(totalClarityMonthlyPaymentsLessDownPayment(data));

            var rbalance = loanTerm !== amortizationTerm ?
                -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, mPayment, 0) *
                (1 + customerRate / 100 / 12) : 0;

            return +formatNumber(rbalance);
        };

        var totalClarityObligation = function (data) {
            var tMonthlyPayments = totalClarityMonthlyPayments(data);
            var rBalance = clarityResidualBalance(data);

            return tMonthlyPayments + rBalance;
        };

        var totalClarityBorrowingCost = function (data) {
            var tObligation = totalClarityObligation(data);
            var tAmountFinanced = totalClarityAmountFinanced(data);
            var includeAdminFee = data.includeAdminFee !== undefined ? data.includeAdminFee : false;
            var adminFee = includeAdminFee ? data.AdminFee : 0;
            var borrowingCost = tObligation - tAmountFinanced - adminFee;
            if (borrowingCost < 0)
                borrowingCost = 0;
            return +borrowingCost.toFixed(2);
        };

        var clarityYourCost = function (data) {
            var yCost = data.DealerCost;

            return yCost * totalClarityAmountFinanced(data) / 100;
        }

        var totalPriceOfEquipment = function (data) {
            var tPrice = totalMonthlyCostOfOwnership(data);

            return tPrice / clarityPaymentFactor;
        }

        var totalClarityMonthlyPaymentsLessDownPayment = function (data) {
            var tPrice = totalMonthlyCostOfOwnership(data);
            var downPayment = data.downPayment;

            return tPrice.toFixed(2) - downPayment * clarityPaymentFactor;
        }

        var totalClarityMCOLessDownPaymentNoTax = function (data) {
            var downPayment = data.downPayment;
            var tMCONoTax = totalMCONoTax(data);
            var totalPriceOfEquipmentNoTax = tMCONoTax / clarityPaymentFactor;
            var totalAmountFinancedNoTax = totalPriceOfEquipmentNoTax - downPayment;

            return totalAmountFinancedNoTax * clarityPaymentFactor;
        }

        return {
            totalMCONoTax: totalMCONoTax,
            totalMonthlyCostOfOwnership: totalMonthlyCostOfOwnership,
            totalObligation: totalClarityObligation,
            totalBorrowingCost: totalClarityBorrowingCost,
            residualBalance: clarityResidualBalance,
            totalMonthlyPayments: totalClarityMonthlyPayments,
            totalAmountFinanced: totalClarityAmountFinanced,
            yourCost: clarityYourCost,
            totalPriceOfEquipment: totalPriceOfEquipment,
            tax: clarityTax,
            totalMonthlyPaymentsLessDownPayment: totalClarityMonthlyPaymentsLessDownPayment,
            totalMCOLessDownPaymentNoTax: totalClarityMCOLessDownPaymentNoTax
        };
    });