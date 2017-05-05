module.exports('home-improvments-view', function (require) {
    var clientActions = require('new-client-actions');
    var createAction = require('redux').createAction;
    var readInitialStateFromFields = require('objectUtils').readInitialStateFromFields;
    var observe = require('redux').observe;

    return function(store) {
        var dispatch = store.dispatch;
        $('span.icon-remove').on('click', 'div.form-group', function (e) {
            dispatch(createAction(clientActions.REMOVE_EQUIPMENT, e.target.value));
            $(this).parent().remove();
        });

        var birth = $("#impvoment-date");

        birth.datepicker({
            dateFormat: 'mm/dd/yy',
            changeYear: true,
            changeMonth: (viewport().width < 768) ? true : false,
            yearRange: '1900:' + (new Date().getFullYear() - 18),
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date(new Date().setFullYear(new Date().getFullYear() - 18)),
            onSelect: function (day) {
                dispatch(createAction(clientActions.SET_IMPROVMENT_MOVE_DATE, day));
            }
        });

        $('#comment').on('change', function(e) {
            dispatch(createAction(clientActions.SET_COMMENT, e.target.value));
        });

        // action handlers
        $('#improvment-equipment').on('change', function (e) {
            var equipmentValue = $(this).val();
            dispatch(createAction(clientActions.SET_NEW_EQUIPMENT, equipmentValue));
            var equipmentText = $("#improvment-equipment :selected").text();
            if (equipmentValue) {
                $('#improvement-types').append($('<li><input class="hidden" name="HomeImprovementTypes" value="' + equipmentValue + '">' + equipmentText + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
            }
        });

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

        var observeCustomerFormStore = observe(store);

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