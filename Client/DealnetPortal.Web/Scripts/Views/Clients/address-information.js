module.exports('address-information-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function (store) {
        var dispatch = store.dispatch;
        var street = $('#street');
        street.on('change', function (e) {
            dispatch(createAction(clientActions.SET_STREET, e.target.value));
        });

        var unit = $('#unit_number');
        unit.on('change', function (e) {
            dispatch(createAction(clientActions.SET_UNIT, e.target.value));
        });

        var city = $('#locality');
        city.on('change', function (e) {
            dispatch(createAction(clientActions.SET_CITY, e.target.value));
        });

        var province = $('#province');
        province.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PROVINCE, e.target.value));
        });

        var postalCode = $('#postal_code');
        postalCode.on('change', function (e) {
            dispatch(createAction(clientActions.SET_POSTAL_CODE, e.target.value));
        });

        $('#clearAddress1').on('click', function (e) {
            e.preventDefault();
            dispatch(createAction(clientActions.CLEAR_ADDRESS, e.target.value));
        });

        var pstreet = $('#previous_street');
        pstreet.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PSTREET, e.target.value));
        });

        var punit = $('#previous_unit_number');
        punit.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PUNIT, e.target.value));
        });

        var pcity = $('#previous_locality');
        pcity.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PCITY, e.target.value));
        });

        var pprovince = $('#previous_administrative_area_level_1');
        pprovince.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PPROVINCE, e.target.value));
        });

        var ppostalCode = $('#previous_postal_code');
        ppostalCode.on('change', function (e) {
            dispatch(createAction(clientActions.SET_PPOSTAL_CODE, e.target.value));
        });

        $('#pclearAddress').on('click', function (e) {
            e.preventDefault();
            dispatch(createAction(clientActions.CLEAR_PADDRESS, e.target.value));
        });

        var lessThanSix = $('#living-time-checkbox');
        lessThanSix.on('click', function (e) {
            dispatch(createAction(clientActions.SET_LESS_THAN_SIX, lessThanSix.prop('checked')));
        });

        var initialStateMap = {
            street: street,
            unit: unit,
            city: city,
            province: province,
            postalCode: postalCode,
            pstreet: pstreet,
            punit: punit,
            pcity: pcity,
            pprovince: pprovince,
            ppostalCode: ppostalCode,
            lessThanSix: lessThanSix
        };

        dispatch(createAction(clientActions.SET_INITIAL_STATE, readInitialStateFromFields(initialStateMap)));

        var observeCustomerFormStore = observe(store);

        observeCustomerFormStore(function (state) {
            return {
                street: state.street,
                unit: state.unit,
                city: state.city,
                province: state.province,
                postalCode: state.postalCode
            };
        })(function (props) {
            street.val(props.street);
            unit.val(props.unit);
            city.val(props.city);
            province.val(props.province);
            postalCode.val(props.postalCode);
        });

        observeCustomerFormStore(function (state) {
            return {
                street: state.pstreet,
                unit: state.punit,
                city: state.pcity,
                province: state.pprovince,
                postalCode: state.ppostalCode
            };
        })(function (props) {
            pstreet.val(props.street);
            punit.val(props.unit);
            pcity.val(props.city);
            pprovince.val(props.province);
            ppostalCode.val(props.postalCode);
        });

        observeCustomerFormStore(function (state) {
            return {
                lessThanSix: state.lessThanSix
            };
        })(function (props) {
            $('#previous-address').find('input, select').each(function () {
                $(this).prop("disabled", !props.lessThanSix);
            });
        });
    };
});
