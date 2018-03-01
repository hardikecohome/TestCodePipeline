module.exports('state', function() {
    var state = {
        agreementType: 0,
        equipments: {},
        packages: {},
        existingEquipments: {},
        tax: taxRate,
        downPayment: 0,
        rentalMPayment: 0,
        clarity: {

        },
        Custom: {
            LoanTerm: '',
            AmortizationTerm: '',
            DeferralPeriod: 0,
            CustomerRate: '',
            yourCost: '',
            DealerCost: 0,
			AdminFee: customerFee
        },
        contractId: 0,
        selectedCardId: null,
        isInitialized: false,
        isNewContract: true
    };

    var constants = {
        rateCards: [{ id: 0, name: 'FixedRate' }, { id: 1, name: 'NoInterest' }, { id: 2, name: 'Deferral' }, { id: 3, name: 'Custom' }],
        customDeferralPeriods: [{ val: 0, name: 'NoDeferral' }, { val: 3, name: 'ThreeMonth' }, { val: 6, name: 'SixMonth' }, { val: 9, name: 'NineMonth' }, { val: 12, name: 'TwelveMonth' }],
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm'],
        minimumLoanValue: 1000,
        amortizationValueToDisable : 180,
		totalAmountFinancedFor180amortTerm: TotalAmtFinancedFor180amortTerm,
        maxRateCardLoanValue: 50000,
        enContactTimeList: ['6 AM', '7 AM', '8 AM', '9 AM', '10 AM', '11 AM', '12 PM', '1 PM', '2 PM', '3 PM', '4 PM', '5 PM', '6 PM', '7 PM', '8 PM'],
        frContactTimeList: ['06:00', '07:00', '08:00', '09:00', '10:00', '11:00', '12:00', '13:00', '14:00', '15:00', '16:00', '17:00', '18:00', '19:00', '20:00']
    };

    return {
        state: state,
        constants: constants
    };
});