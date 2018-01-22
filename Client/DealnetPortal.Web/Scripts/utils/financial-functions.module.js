module.exports('financial-functions',
    function() {
        var tax = function(data) {
            return data.equipmentSum * data.tax / 100;
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

        var totalRentalAmountFinanced = function(data) {
            var tPrice = totalRentalPrice(data);
            var adminFee = data.AdminFee;
            var downPayment = data.downPayment;

            return tPrice/* + adminFee*/ - downPayment;
        }

        var rentalMonthlyPayment = function(data) {
            var tAmountFinanced = totalRentalAmountFinanced(data);
            var amortizationTerm = data.AmortizationTerm;
            var customerRate = data.CustomerRate;

            return (Math.round((tAmountFinanced * pmt(customerRate / 100 / 12, amortizationTerm, -1, 0, 0))*100)/100);
        };

        var totalRentalMonthlyPayments = function(data) {
            var mPayment = rentalMonthlyPayment(data);
            var loanTerm = data.LoanTerm;

            return mPayment * loanTerm;
        };

        var rentalResidualBalance = function(data) {
            var amortizationTerm = data.AmortizationTerm;
            var loanTerm = data.LoanTerm;
            var customerRate = data.CustomerRate;
            var mPayment = rentalMonthlyPayment(data);

            var rbalance = 0;
            if (loanTerm !== amortizationTerm) {
                rbalance = -pv(customerRate / 100 / 12, amortizationTerm - loanTerm, mPayment, 0) *
                    (1 + customerRate / 100 / 12);
            }

            return rbalance;
        };

        var totalRentalObligation = function(data) {
            var tMonthlyPayments = totalRentalMonthlyPayments(data);
            var rBalance = rentalResidualBalance(data);
            var adminFee = data.AdminFee;
            return tMonthlyPayments + rBalance + adminFee;
        };

        var totalRentalBorrowingCost = function(data) {
            var tObligation = totalRentalObligation(data);
            var tAmountFinanced = totalRentalAmountFinanced(data);
            var adminFee = data.AdminFee;
            var borrowingCost = tObligation - tAmountFinanced - adminFee;
            if (borrowingCost < 0)
                borrowingCost = 0;
            return borrowingCost;
        };

        var rentalYourCost = function (data) {
            var yCost = data.DealerCost;

            return yCost * totalRentalAmountFinanced(data) / 100;
        }

        return {
            tax: tax,
			totalPrice: totalPrice,
			totalRentalPrice: totalRentalPrice,
            totalRentalObligation: totalRentalObligation,
            totalRentalBorrowingCost: totalRentalBorrowingCost,
            totalObligation: totalObligation,
            residualBalance: residualBalance,
            rentalResidualBalance: rentalResidualBalance,
            totalMonthlyPayments: totalMonthlyPayments,
            monthlyPayment: monthlyPayment,
            totalRentalMonthlyPayments: totalRentalMonthlyPayments,
            rentalMonthlyPayment: rentalMonthlyPayment,
            totalAmountFinanced: totalAmountFinanced,
            totalRentalAmountFinanced: totalRentalAmountFinanced,
            totalBorrowingCost: totalBorrowingCost,
            yourCost: yourCost,
            rentalYourCost: rentalYourCost
        };
    });