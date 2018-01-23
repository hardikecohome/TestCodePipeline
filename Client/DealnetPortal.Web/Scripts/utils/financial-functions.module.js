module.exports('financial-functions',
    function() {
        var tax = function(data) {
            return data.equipmentSum * data.tax / 100;
        };

        var clarityTax = function(data) {
            return (data.packagesSum + data.equipmentSum) * data.tax / 100;
        };

        var totalClarityPrice = function (data) {
            var t = clarityTax(data);
            var equipmentSum = data.equipmentSum;
            var packagesSum = data.packagesSum;

            return packagesSum + equipmentSum + t;
        };

        var totalRentalPrice = function (data) {
			var t = tax(data);
			var equipmentSum = data.equipmentSum;

			return equipmentSum + t;
		};


        var totalPrice = function(data) {
            //var t = tax(data);
            var equipmentSum = data.equipmentSum;

            return equipmentSum /*+ t*/;
        };

        var totalAmountFinanced = function(data) {
            var tPrice = totalPrice(data);
            var adminFee = data.AdminFee;
            var downPayment = data.downPayment;

            return tPrice/* + adminFee*/ - downPayment;
        };

        var yourCost = function (data) {
            var yCost = data.DealerCost;

            return yCost * totalAmountFinanced(data) / 100;
        }

        var monthlyPayment = function(data) {
            var tAmountFinanced = totalAmountFinanced(data);
            var amortizationTerm = data.AmortizationTerm;
            var customerRate = data.CustomerRate;

			return customerRate > 0 ? (Math.round((tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0)) * 100) / 100) 
				: (tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0));
        };

        var totalMonthlyPayments = function(data) {
            var mPayment = monthlyPayment(data);
            var loanTerm = data.LoanTerm;

            return mPayment * loanTerm;
        };

        var residualBalance = function(data) {
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

        var totalObligation = function(data) {
            var tMonthlyPayments = totalMonthlyPayments(data);
            var rBalance = residualBalance(data);
			var adminFee = data.AdminFee;
			return tMonthlyPayments + rBalance + adminFee;
        };

        var totalBorrowingCost = function(data) {
            var tObligation = totalObligation(data);
			var tAmountFinanced = totalAmountFinanced(data);
			var adminFee = data.AdminFee;
			var borrowingCost = tObligation - tAmountFinanced - adminFee;
            if (borrowingCost < 0)
                borrowingCost = 0;
            return borrowingCost;
        };

        var totalClarityAmountFinanced = function(data) {
            var tPrice = totalClarityPrice(data);
            var adminFee = data.AdminFee;
            var downPayment = data.downPayment;

            return tPrice/* + adminFee*/ - downPayment;
        }

        var clarityMonthlyPayment = function(data) {
            var tAmountFinanced = totalClarityAmountFinanced(data);
            var amortizationTerm = data.AmortizationTerm;
            var customerRate = data.CustomerRate;

            return (Math.round((tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0))*100)/100);
        };

        var totalClarityMonthlyPayments = function(data) {
            var mPayment = totalClarityAmountFinanced(data);
            var loanTerm = data.LoanTerm;

            return mPayment * loanTerm;
        };

        var clarityResidualBalance = function(data) {
            var amortizationTerm = data.AmortizationTerm;
            var loanTerm = data.LoanTerm;
            var customerRate = data.CustomerRate;
            var mPayment = totalClarityAmountFinanced(data);

            var rbalance = 0;
            if (loanTerm !== amortizationTerm) {
                rbalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, mPayment, 0) *
                    (1 + customerRate / 100 / 12);
            }

            return rbalance;
        };

        var totalClarityObligation = function(data) {
            var tMonthlyPayments = totalClarityMonthlyPayments(data);
            var rBalance = clarityResidualBalance(data);
            var adminFee = data.AdminFee;
            return tMonthlyPayments + rBalance + adminFee;
        };

        var totalClarityBorrowingCost = function(data) {
            var tObligation = totalClarityObligation(data);
            var tAmountFinanced = totalClarityAmountFinanced(data);
            var adminFee = data.AdminFee;
            var borrowingCost = tObligation - tAmountFinanced - adminFee;
            if (borrowingCost < 0)
                borrowingCost = 0;
            return borrowingCost;
        };

        var clarityYourCost = function (data) {
            var yCost = data.DealerCost;

            return yCost * totalClarityAmountFinanced(data) / 100;
        }

        var rentalTax = function(data) {
            return data.equipmentSum * data.tax / 100;
        };

        return {
            tax: tax,
			totalPrice: totalPrice,
			totalRentalPrice: totalRentalPrice,
            totalRentalObligation: totalClarityObligation,
            totalRentalBorrowingCost: totalClarityBorrowingCost,
            totalObligation: totalObligation,
            residualBalance: residualBalance,
            rentalResidualBalance: clarityResidualBalance,
            totalMonthlyPayments: totalMonthlyPayments,
            monthlyPayment: monthlyPayment,
            totalRentalMonthlyPayments: totalClarityMonthlyPayments,
            rentalMonthlyPayment: clarityMonthlyPayment,
            totalAmountFinanced: totalAmountFinanced,
            totalRentalAmountFinanced: totalClarityAmountFinanced,
            totalBorrowingCost: totalBorrowingCost,
            yourCost: yourCost,
            rentalYourCost: clarityYourCost
        };
    });