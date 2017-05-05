module.exports('new-client-flow', function(require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;

    return function (store) {
        return function (next) {
            var basicInfoFlow = [clientActions.SET_NAME, clientActions.SET_LAST, clientActions.SET_BIRTH];
            var additionalInfoFlow = [clientActions.SET_STREET, clientActions.SET_CITY, clientActions.SET_PROVINCE, clientActions.SET_POSTAL_CODE];
            var contactInfoFlow = [clientActions.SET_PHONE, clientActions.SET_CELL_PHONE, clientActions.SET_EMAIL, clientActions.SET_CONTACT_METHOD];
            var addressFlow = [clientActions.SET_STREET, clientActions.SET_CITY, clientActions.SET_PROVINCE, clientActions.SET_POSTAL_CODE];
            var homeImprovmentFlow = [clientActions.SET_NEW_EQUIPMENT];

            return function (action) {
                var state = store.getState();

                var nextAction = next(action);
                if (state.activePanel === 'basic-information') {
                    var index1 = basicInfoFlow.indexOf(action.type);
                    if (index1 >= 0) {
                        basicInfoFlow.splice(index1, 1);
                    }
                    if (basicInfoFlow.length === 0) {
                        next(createAction(clientActions.ACTIVATE_ADDRESS_INFO, true));
                    }
                }

                if (state.activePanel === 'additional-infomration') {
                    var index2 = additionalInfoFlow.indexOf(action.type);
                    if (index2 >= 0) {
                        additionalInfoFlow.splice(index2, 1);
                    }
                    if (action.type === clientActions.SET_ADDRESS && action.payload.province !== '') {
                        addressFlow.forEach(function (action) {
                            var index4 = additionalInfoFlow.indexOf(action);
                            if (index4 >= 0) {
                                additionalInfoFlow.splice(index4, 1);
                            }
                        });
                    }

                    if (additionalInfoFlow.length === 0) {
                        next(createAction(clientActions.ACTIVATE_CONTACT_INFO, true));
                    }
                }

                if (state.activePanel === 'contact-information') {
                    var index3 = contactInfoFlow.indexOf(action.type);
                    if (index3 >= 0) {
                        contactInfoFlow.splice(index3, 1);
                    }

                    if (contactInfoFlow.length === 0) {
                        next(createAction(clientActions.ACTIVATE_HOME_IMPROVMENTS, true));
                    }
                }

                if (state.activePanel === 'home-improvments') {
                    var index4 = homeImprovmentFlow.indexOf(action.type);
                    if (index4 >= 0) {
                        homeImprovmentFlow.splice(index4, 1);
                    }

                    if (homeImprovmentFlow.length === 0) {
                        next(createAction(clientActions.ACTIVATE_CLIENT_CONSENTS, true));
                    }
                }

                var stateAfter = store.getState();
                var errors = [{ type: '' }];
                var installationErrors = errors.filter(function (error) { return error.type === 'ownership'; });
                if (stateAfter.displaySubmitErrors && installationErrors.length > 0) {
                    next(createAction(clientActions.DISPLAY_ADDRESS_INFO, true));
                }

                var contactInfoErrors = errors.filter(function (error) {
                    return ['agreement'].some(function (item) {
                        return error.type === item;
                    });
                });

                if (stateAfter.displaySubmitErrors && contactInfoErrors.length > 0) {
                    next(createAction(clientActions.DISPLAY_CONTACT_INFO, true));
                }

                return nextAction;
            };
        };
    };
});