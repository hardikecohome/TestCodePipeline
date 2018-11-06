module.exports('customer-selectors', function (require) {
    var filterObj = require('objectUtils').filterObj;
    var mapObj = require('objectUtils').mapObj;

    var getRequiredPhones = function (state) {
        return {
            phone: state.cellPhone === '' || (state.cellPhone !== '' && state.phone !== ''),
            cellPhone: state.phone === ''
        };
    };

    var isEmployedOrSelfEmployed = function (state) {
        return state.employStatus == 0 || state.employStatus == 2;
    }
    var getRequiredEmpoyment = function (state) {
        return state.isQuebecDealer ? {
            employStatus: state.employStatus === '',
            incomeType: state.employStatus === 0 && state.incomeType == '',
            annualSalary: state.employStatus === 0 && state.incomeType === 0 && state.annualSalary == '' || true,
            hourlyRate: (state.employStatus == 0 && state.incomeType == 1 && state.hourlyRate == '') || false,
            yearsOfEmploy: isEmployedOrSelfEmployed(state) && state.yearsOfEmploy == '',
            monthsOfEmploy: isEmployedOrSelfEmployed(state) && state.yearsOfEmploy != '10+' && state.monthsOfEmploy == '',
            employType: state.employStatus == 0 && state.employType == '',
            jobTitle: isEmployedOrSelfEmployed(state) && state.jobTitle == '',
            companyName: isEmployedOrSelfEmployed(state) && state.companyName == '',
            companyPhone: isEmployedOrSelfEmployed(state) && state.companyPhone == '',
            cstreet: isEmployedOrSelfEmployed(state) && state.cstreet == '',
            ccity: isEmployedOrSelfEmployed(state) && state.ccity == '',
            cprovince: isEmployedOrSelfEmployed(state) && state.cprovince == '',
            cpostalCode: isEmployedOrSelfEmployed(state) && state.cpostalCode == ''
        } : {};
    };

    var getErrors = function (requiredFields, requiredPFields) {
        return function (state) {
            var errors = [];

            var isQuebecAddress = state.province.toLowerCase() === 'qc';

            //if (state.birthday !== '') {
            //    var ageDifMs = Date.now() - Date.parseExact(state.birthday, 'M/d/yyyy');
            //    var ageDate = new Date(ageDifMs);
            //    var age = Math.abs(ageDate.getUTCFullYear() - 1970);

            //    if (age > 75) {
            //        errors.push({
            //            type: 'birthday',
            //            messageKey: 'YouShouldBe75OrLess',
            //        });
            //    }
            //}

            if (state.isQuebecDealer) {
                if (state.province !== '' && !isQuebecAddress) {
                    errors.push({
                        type: 'quebec',
                        messageKey: 'InstallationAddressInQuebec'
                    });
                }
            }
            if (!state.isQuebecDealer) {
                if (state.province !== '' && isQuebecAddress) {
                    errors.push({
                        type: 'quebec',
                        messageKey: 'InstallationAddressCannotInQuebec'
                    });
                }
            }

            var requiredPhones = filterObj(function (key, obj) {
                return obj[key];
            })(getRequiredPhones(state));

            var requiredP = state.lessThanSix ? requiredPFields : [];

            var requiredE = isQuebecAddress ? filterObj(function (key, obj) {
                return obj[key];
            })(getRequiredEmpoyment(state)) : [];

            var emptyErrors = requiredFields.concat(requiredPhones).concat(requiredP).concat(requiredE).map(mapObj(state))
                .some(function (val) {
                    return typeof val === 'string' ? val === '' : !val;
                });

            if (emptyErrors) {
                errors.push({
                    type: 'empty',
                    messageKey: 'FillRequiredFields'
                });
            }

            return errors;
        };
    };

    return {
        getRequiredPhones: getRequiredPhones,
        getErrors: getErrors
    };
});