module.exports('home-improvments-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;
    var improvments = [];
    return function(store) {
        var dispatch = store.dispatch;
        $('span.icon-remove').on('click', 'div.form-group', function (e) {
            dispatch(createAction(clientActions.REMOVE_EQUIPMENT, e.target.value));
            $(this).parent().remove();
        });

        var improvmentMoveInDate = $("#impvoment-date");
        inputDateFocus(improvmentMoveInDate);
        improvmentMoveInDate.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:2200',
            minDate: new Date(),
            onSelect: function (day) {
                onDateSelect($(this));
                dispatch(createAction(clientActions.SET_IMPROVMENT_MOVE_DATE, day));
            }
        });

        $('#comment').on('change', function(e) {
            dispatch(createAction(clientActions.SET_COMMENT, e.target.value));
        });

        // action handlers
        $('#improvment-equipment').mouseup(function() {
            var open = $(this).data("isopen");

            if (open) {
                var equipmentValue = $(this).val();
                dispatch(createAction(clientActions.SET_NEW_EQUIPMENT, equipmentValue));
                var equipmentText = $("#improvment-equipment :selected").text();
                if (equipmentValue) {
                    $('#improvement-types').append($('<li><input class="hidden" name="HomeImprovementTypes" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
                }

            }

            $(this).data("isopen", !open);
        });

        //$('#improvment-equipment').on('change', function (e) {
        //    var equipmentValue = $(this).val();

        //    var equipmentText = $("#improvment-equipment :selected").text();
        //    if (equipmentValue) {
        //        $('#improvement-types').append($('<li><input class="hidden" name="HomeImprovementTypes" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
        //    }
            
        //});

        var houseCustomer = $('#houseCustomerChosen');
        houseCustomer.on('click', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_OTHER_ADDRESS, houseCustomer.prop('checked')));
        });

        var unknownAddress = $('#addressUnknownCheckbox');
        unknownAddress.on('click', function (e) {
            dispatch(createAction(clientActions.SET_UNKNOWN_ADDRESS, unknownAddress.prop('checked')));
        });

        var currentAddress = $('#currentAddressIsMortgage');

        currentAddress.on('click', function (e) {
            dispatch(createAction(clientActions.SET_CURRENT_ADDRESS, currentAddress.prop('checked')));
        });

        $('#clearImprovmentAddress').on('click', function (e) {
            e.preventDefault();
            dispatch(createAction(clientActions.CLEAR_IMPROVMENT_ADDRESS, e.target.value));
        });

        var street = $('#improvment_street');
        street.on('change', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_STREET, e.target.value));
        });

        var unit = $('#improvment_unit_number');
        unit.on('change', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_UNIT, e.target.value));
        });

        var city = $('#improvment_locality');
        city.on('change', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_CITY, e.target.value));
        });

        var province = $('#improvment_administrative_area_level_1');
        province.on('change', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_PROVINCE, e.target.value));
        });

        var postalCode = $('#improvment_postal_code');
        postalCode.on('change', function (e) {
            dispatch(createAction(clientActions.SET_IMPROVMENT_POSTAL_CODE, e.target.value));
        });

        var observeCustomerFormStore = observe(store);

        observeCustomerFormStore(function (state) {
            return {
                street: state.improvmentStreet,
                unit: state.improvmentUnit,
                city: state.improvmentCity,
                province: state.improvmentProvince,
                postalCode: state.improvmentPostalCode,
                moveInDate: state.improvmentMoveInDate
            };
        })(function (props) {
            street.val(props.street);
            unit.val(props.unit);
            city.val(props.city);
            province.val(props.province);
            postalCode.val(props.postalCode);
            improvmentMoveInDate.val(props.moveInDate);
        });

        observeCustomerFormStore(function (state) {
            return {
                unknownAddress: state.unknownAddress
            };
        })(function (props) {
            $('#installation-address').find('input:not(:checkbox), select').each(function () {
                $(this).prop("disabled", props.unknownAddress);
            });
            
        });
    }
});