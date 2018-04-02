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
        return {
            employStatus: true,
            incomeType: state.employStatus == 0,
            annualSalary: state.employStatus == 0 && state.incomeType == 0 || true,
            hourlyRate: state.employStatus == 0 && state.incomeType == 1 || false,
            yearsOfEmploy: isEmployedOrSelfEmployed(state),
            monthsOfEmploy: isEmployedOrSelfEmployed(state) &&
                state.yearsOfEmploy != '10+',
            employType: state.employStatus == 0,
            jobTitle: isEmployedOrSelfEmployed(state),
            companyName: isEmployedOrSelfEmployed(state),
            companyPhone: isEmployedOrSelfEmployed(state),
            cstreet: isEmployedOrSelfEmployed(state),
            ccity: isEmployedOrSelfEmployed(state),
            cprovince: isEmployedOrSelfEmployed(state),
            cpostalCode: isEmployedOrSelfEmployed(state)
        };
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