module.exports('customer-selectors', function (require) {
    var filterObj = require('objectUtils').filterObj;
    var mapObj = require('objectUtils').mapObj;

    var getRequiredPhones = function (state) {
        return {
            phone: state.cellPhone === '' || (state.cellPhone !== '' && state.phone !== ''),
            cellPhone: state.phone === '',
        };
    };

    var getErrors = function (requiredFields, requiredPFields) {
        return function (state) {
            var errors = [];

            if (state.birthday !== '') {
                var ageDifMs = Date.now() - Date.parseExact(state.birthday, "M/d/yyyy");
                var ageDate = new Date(ageDifMs);
                var age = Math.abs(ageDate.getUTCFullYear() - 1970);

                if (age > 75) {
                    errors.push({
                        type: 'birthday',
                        messageKey: 'YouShouldBe75OrLess',
                    });
                }
            }

            var requiredPhones = filterObj(function (key, obj) {
                return obj[key];
            })(getRequiredPhones(state));

            var requiredP = state.lessThanSix ? requiredPFields : [];

            var emptyErrors = requiredFields.concat(requiredPhones).concat(requiredP).map(mapObj(state))
                .some(function (val) {
                    return typeof val === 'string' ? val === '' : !val;
                });

            if (emptyErrors) {
                errors.push({
                    type: 'empty',
                    messageKey: 'FillRequiredFields',
                });
            }

            return errors;
        };
    };

    return {
        getRequiredPhones: getRequiredPhones,
        getErrors: getErrors,
    };
});