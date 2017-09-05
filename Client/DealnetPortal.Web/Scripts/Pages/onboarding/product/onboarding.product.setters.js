module.exports('onboarding.product.setters', function (require) {
    var state = require('onboarding.state').state;

    var configSetField = require('onboarding.setters').configSetField;
    var moveToNextSection = require('onboarding.setters').moveToNextSection;

    var stateSection = 'product';
    var oem = 'oem';
    var financeProviderName = 'finance-provider-name';
    var monthlyFinancedValue = 'monthly-financed-value';
    var percentMonthDefferals = 'percent-month-deferrals';
    var equipment = 'equipment';

    var setFormField = configSetField(stateSection);

    var setPrimaryBrand = setFormField('primary-brand');

    var addSecondaryBrand = function (value) {
        if (!state[stateSection].brands)
            state[stateSection].brands = [];
        if (state[stateSection].brands.length > 1)
            return false;
        state[stateSection].brands.push(value);
        return true;
    }

    var setSecondaryBrand = function (index, value) {
        state[stateSection].brands[index] = value;
    };

    var removeSecondayBrand = function (index) {
        if (state[stateSection].brands.length - 1 >= index) {
            state[stateSection].brands.splice(index, 1);
            return true;
        }
        return false;
    }

    var setAnnualSales = setFormField('annual-sales-volume');

    var setTransactionSize = setFormField('av-transaction-size');

    var setSalesApproach = function (e) {
        if (!state[stateSection].salesApproch) {
            state[stateSection].salesApproch = [];
        }
        var index = state[stateSection].salesApproch.indexOf(e.target.id);
        if (e.target.checked) {
            if (index === -1)
                state[stateSection].salesApproch.push(e.target.id);
        } else {
            if (index > -1) {
                state[stateSection].salesApproch.splice(index, 1);
            }
        }
    }

    var setLeadGen = function (e) {
        if (!state[stateSection].leadGen) {
            state[stateSection].leadGen = [];
        }
        var index = state[stateSection].leadGen.indexOf(e.target.id);
        if (e.target.checked) {
            if (index === -1)
                state[stateSection].leadGen.push(e.target.id);
        } else {
            if (index > -1) {
                state[stateSection].leadGen.splice(index, 1);
            }
        }
    }

    var setProgramService = function (id) {
        state[stateSection].programService = id;

        moveToNextSection(stateSection);
    }

    var setRelationship = setFormField('relationship');

    var setOem = setFormField(oem);

    var setWithCurrentProvider = function (e) {
        if (e.target.checked) {
            var index = state[stateSection].requiredFields.indexOf(financeProviderName);
            if (index === -1 && !$('#' + financeProviderName).valid())
                state[stateSection].requiredFields.push(financeProviderName);
            index = state[stateSection].requiredFields.indexOf(monthlyFinancedValue);
            if (index === -1 && !$('#' + monthlyFinancedValue).valid())
                state[stateSection].requiredFields.push(monthlyFinancedValue);
        } else {
            var index = state[stateSection].requiredFields.indexOf(financeProviderName);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
            index = state[stateSection].requiredFields.indexOf(monthlyFinancedValue);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
        }
    }

    var setFinanceProviderName = setFormField(financeProviderName);

    var setMonthFinancedValue = setFormField(monthlyFinancedValue);

    var setOfferDeferrals = function (e) {
        if (e.target.checked) {
            var index = state[stateSection].requiredFields.indexOf(percentMonthDefferals);
            if (index === -1 && !$('#' + percentMonthDefferals).valid())
                state[stateSection].requiredFields.push(percentMonthDefferals);
        } else {
            var index = state[stateSection].requiredFields.indexOf(percentMonthDefferals);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
        }
    }

    var setPercentMonthDeferrals = setFormField(percentMonthDefferals);

    var equipmentAdded = function (e) {
        var id = Number(e.target.value);
        var index = state[stateSection].selectedEquipment.indexOf(id);

        if (index === -1) {
            state[stateSection].selectedEquipment.push(id);
            var requiredIndex = state[stateSection].requiredFields.indexOf(equipment);
            if (requiredIndex > -1) {
                state[stateSection].requiredFields.splice(requiredIndex, 1);
                moveToNextSection(stateSection);
            }
            return true;
        }
        return false;
    }

    var equipmentRemoved = function (id) {
        var value = Number(id);
        var index = state[stateSection].selectedEquipment.indexOf(value);
        if (index > -1) {
            state[stateSection].selectedEquipment.splice(index, 1);
            if (state[stateSection].selectedEquipment.length < 1) {
                var requiredIndex = state[stateSection].requiredFields.indexOf(equipment);
                if (requiredIndex > -1) {
                    state[stateSection].requiredFields.splice(requiredIndex, 1);
                }
                $('#equipment-error').removeClass('hidden');
            }
            return true;
        }
        return false;
    }

    var setReasonForInterest = setFormField('reason-for-interest');

    return {
        setPrimaryBrand: setPrimaryBrand,
        addSecondaryBrand: addSecondaryBrand,
        setSecondaryBrand: setSecondaryBrand,
        removeSecondayBrand: removeSecondayBrand,
        setAnnualSales: setAnnualSales,
        setTransactionSize: setTransactionSize,
        setSalesApproach: setSalesApproach,
        setLeadGen: setLeadGen,
        setProgramService: setProgramService,
        setRelationship: setRelationship,
        setOem: setOem,
        setWithCurrentProvider: setWithCurrentProvider,
        setFinanceProviderName: setFinanceProviderName,
        setMonthFinancedValue: setMonthFinancedValue,
        setOfferDeferrals: setOfferDeferrals,
        setPercentMonthDeferrals: setPercentMonthDeferrals,
        equipmentAdded: equipmentAdded,
        equipmentRemoved: equipmentRemoved,
        setReasonForInterest: setReasonForInterest
    };
});
