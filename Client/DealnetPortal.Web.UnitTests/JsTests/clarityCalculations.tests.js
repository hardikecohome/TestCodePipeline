describe('clarity calculations', function () {
    translations = {
        ThisFieldIsRequired: '',
        EnterValidDate: ''
    };
    var clarityFunctions = module.require('financial-functions.clarity');
    var state = {}

    beforeEach(function () {
        state = {
            equipmentSum: 0,
            packagesSum: 0,
            downPayment: 0,
            LoanTerm: 60,
            AmortizationTerm: 144,
            CustomerRate: 6.95,
            AdminFee: 0,
            DealerCost: 0,
            tax: 13
        };
    });

    describe('tax', function () {
        it('equals 8.96 when equipmentSum == 68.95', function () {
            state.equipmentSum = 68.95;

            var result = clarityFunctions.tax(state);

            expect(+result.toFixed(2)).toEqual(8.96);
        });

        it('equals 3.63 when equipmentSum == 27.95', function () {
            state.equipmentSum = 27.95;

            var result = clarityFunctions.tax(state);

            expect(+result.toFixed(2)).toEqual(3.63);
        });

        it('equals 13.24 when equipmentSum == 101.98', function () {
            state.equipmentSum = 101.98;

            var result = clarityFunctions.tax(state);

            expect(+result.toFixed(2)).toEqual(13.26);
        });
    });

    describe('totalMonthlyCostOfOwnership', function () {
        it('equals 77.91 when equipmentSum == 68.95', function () {
            state.equipmentSum = 68.95;

            var result = clarityFunctions.totalMonthlyCostOfOwnership(state);

            expect(+result.toFixed(2)).toEqual(77.91);
        });

        it('equals 31.58 when equipmentSum == 27.95', function () {
            state.equipmentSum = 27.95;

            var result = clarityFunctions.totalMonthlyCostOfOwnership(state);

            expect(+result.toFixed(2)).toEqual(31.58);
        });

        it('equals 115.24 when equipmentSum == 101.98', function () {
            state.equipmentSum = 101.98;

            var result = clarityFunctions.totalMonthlyCostOfOwnership(state);

            expect(+result.toFixed(2)).toEqual(115.24);
        });
    });

    describe('totalMonthlyPaymentsLessDownPayment', function () {
        it('equals 67.66 when equipmentSum == 68.95 && downPayment == 1000', function () {
            state.equipmentSum = 68.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalMonthlyPaymentsLessDownPayment(state);

            expect(+result.toFixed(2)).toEqual(67.66);
        });

        it('equals 21.33 when equipmentSum == 27.95 && downPayment == 1000', function () {
            state.equipmentSum = 27.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalMonthlyPaymentsLessDownPayment(state);

            expect(+result.toFixed(2)).toEqual(21.33);
        });

        it('equals 84.47 when equipmentSum == 101.98 && downPayment == 3000', function () {
            state.equipmentSum = 101.98;
            state.downPayment = 3000;

            var result = clarityFunctions.totalMonthlyPaymentsLessDownPayment(state);

            expect(+result.toFixed(2)).toEqual(84.47);
        });
    });

    describe('totalPriceOfEquipment', function () {
        it('equals 7596.13 when equipmentSum == 68.95 && downPayment == 1000', function () {
            state.equipmentSum = 68.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalPriceOfEquipment(state);

            expect(+result.toFixed(2)).toEqual(7596.13);
        });

        it('equals 3079.21 when equipmentSum == 27.95 && downPayment == 1000', function () {
            state.equipmentSum = 27.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalPriceOfEquipment(state);

            expect(+result.toFixed(2)).toEqual(3079.21);
        });

        it('equals 11235.00 when equipmentSum == 101.98 && downPayment == 3000', function () {
            state.equipmentSum = 101.98;
            state.downPayment = 3000;

            var result = clarityFunctions.totalPriceOfEquipment(state);

            expect(+result.toFixed(2)).toEqual(11235.00);
        });
    });

    describe('totalObligation', function () {
        it('equals 8575.61 when equipmentSum == 68.95 && downPayment == 1000', function () {
            state.equipmentSum = 68.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalObligation(state);

            expect(+result.toFixed(2)).toEqual(8575.61);
        });

        it('equals 2703.32 when equipmentSum == 27.95 && downPayment == 1000', function () {
            state.equipmentSum = 27.95;
            state.downPayment = 1000;

            var result = clarityFunctions.totalObligation(state);

            expect(+result.toFixed(2)).toEqual(2703.32);
        });

        it('equals 10706.25 when equipmentSum == 101.98 && downPayment == 3000', function () {
            state.equipmentSum = 101.98;
            state.downPayment = 3000;

            var result = clarityFunctions.totalObligation(state);

            expect(+result.toFixed(2)).toEqual(10706.25);
        });
    });
});