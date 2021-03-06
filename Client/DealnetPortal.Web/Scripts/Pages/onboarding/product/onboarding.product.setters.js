module.exports('onboarding.product.setters', function (require) {
    var state = require('onboarding.state').state;

    var configSetField = require('onboarding.setters').configSetField;
    var moveToNextSection = require('onboarding.setters').moveToNextSection;
    var enableSubmit = require('onboarding.setters').enableSubmit;

    var stateSection = 'product';
    var oem = 'oem';
    var financeProviderName = 'finance-provider-name';
    var monthlyFinancedValue = 'monthly-financed-value';
    var percentMonthDefferals = 'percent-month-deferrals';
    var equipment = 'equipment';

    var setFormField = configSetField(stateSection);

    var setPrimaryBrand = setFormField('primary-brand');

    var initSecondaryBrands = function (value) {
        if (!state[stateSection].brands)
            state[stateSection].brands = [];
    }

    var setSecondaryBrand = function (index) {
        return function (e) {
            state[stateSection].brands[index] = e.target.value;
        }
    };

    var removeSecondayBrand = function (index) {
        var item = state[stateSection].brands[index];

        if (item) {
            state[stateSection].brands.splice(index, 1);
        }
    }

    var setAnnualSales = setFormField('annual-sales-volume');

    var setTransactionSize = setFormField('av-transaction-size');

    var setSalesApproach = function (e) {
        if (!state[stateSection].salesApproch) {
            state[stateSection].salesApproch = [];
        }
        var index = state[stateSection].salesApproch.indexOf(e.target.id);
        if (e.target.checked) {
            if (index === -1) {
                state[stateSection].salesApproch.push(e.target.id);
                var requiredIndex = state[stateSection].requiredFields.indexOf('sales-approach');
                if (requiredIndex > -1)
                    state[stateSection].requiredFields.splice(requiredIndex, 1);
            }
        } else {
            if (index > -1) {
                state[stateSection].salesApproch.splice(index, 1);
                if (state[stateSection].salesApproch.length === 0) {
                    var requiredIndex = state[stateSection].requiredFields.indexOf('sales-approach');
                    if (requiredIndex === -1)
                        state[stateSection].requiredFields.push('sales-approach');
                }
            }
        }

        moveToNextSection(stateSection);
        enableSubmit();
    }

    var setLeadGen = function (e) {
        if (!state[stateSection].leadGen) {
            state[stateSection].leadGen = [];
        }
        var index = state[stateSection].leadGen.indexOf(e.target.id);
        if (e.target.checked) {
            if (index === -1) {
                state[stateSection].leadGen.push(e.target.id);
                var requiredIndex = state[stateSection].requiredFields.indexOf('lead-gen');
                if (requiredIndex > -1)
                    state[stateSection].requiredFields.splice(requiredIndex, 1);
            }
        } else {
            if (index > -1) {
                state[stateSection].leadGen.splice(index, 1);
                if (state[stateSection].leadGen.length === 0) {
                    var requiredIndex = state[stateSection].requiredFields.indexOf('lead-gen');
                    if (requiredIndex === -1)
                        state[stateSection].requiredFields.push('lead-gen');
                }
            }
        }
        moveToNextSection(stateSection);
        enableSubmit();
    }

    var setProgramService = function (id) {
        state[stateSection].programService = id;

        var index = state[stateSection].requiredFields.indexOf('program-service');
        if (index > -1)
            state[stateSection].requiredFields.splice(index, 1);

        moveToNextSection(stateSection);
        enableSubmit();
    }

    var setRelationship = function (e) {
        var index = state[stateSection].requiredFields.indexOf('relationship');
        if (index > -1)
            state[stateSection].requiredFields.splice(index, 1);
        if (e.target.value === "1") {
            index = state[stateSection].requiredFields.indexOf(oem);
            if (index === -1 && !state[stateSection][oem]) {
                state[stateSection].requiredFields.push(oem);
            }
        } else {
            index = state[stateSection].requiredFields.indexOf(oem);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
        }

        moveToNextSection(stateSection);
        enableSubmit();
    }

    var setOem = setFormField(oem);

    var setWithCurrentProvider = function (e) {
        if (e.target.checked) {
            var index = state[stateSection].requiredFields.indexOf(financeProviderName);
            if (index === -1 && !state[stateSection][financeProviderName])
                state[stateSection].requiredFields.push(financeProviderName);
            index = state[stateSection].requiredFields.indexOf(monthlyFinancedValue);
            if (index === -1 && !state[stateSection][monthlyFinancedValue])
                state[stateSection].requiredFields.push(monthlyFinancedValue);
        } else {
            var index = state[stateSection].requiredFields.indexOf(financeProviderName);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
            index = state[stateSection].requiredFields.indexOf(monthlyFinancedValue);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
        }

        moveToNextSection(stateSection);
        enableSubmit();
    }

    var setFinanceProviderName = setFormField(financeProviderName);

    var setMonthFinancedValue = setFormField(monthlyFinancedValue);

    var setOfferDeferrals = function (e) {
        if (e.target.checked) {
            var index = state[stateSection].requiredFields.indexOf(percentMonthDefferals);
            if (index === -1 && !state[stateSection][percentMonthDefferals])
                state[stateSection].requiredFields.push(percentMonthDefferals);
        } else {
            var index = state[stateSection].requiredFields.indexOf(percentMonthDefferals);
            if (index > -1)
                state[stateSection].requiredFields.splice(index, 1);
        }

        moveToNextSection(stateSection);
        enableSubmit();
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
                enableSubmit();
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
                if (requiredIndex === -1) {
                    state[stateSection].requiredFields.push(equipment);
                    enableSubmit();
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
        initSecondaryBrands: initSecondaryBrands,
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
