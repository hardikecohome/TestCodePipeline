module.exports('postalCode-handlers', function (require) {
    var template = require('postalCode-template');
    var state = require('my-profile-state');

    var init = function () {
        var postalCodes = $('div#postal-code-area').find('[id^=postal-code-]');
        for (var i = 0; i < postalCodes.length; i++) {
            var value = $(postalCodes[i]).find('#PostalCodes_' + i + '__PostalCode').val();
            var item = { id: i, value: value };
            setHandlers(item);
        }
    }

    var add = function () {
        var newTemplate = template($('<div></div>'), { id: state.postalCodeSecondId });
        $('#postal-code-area').append(newTemplate);

        setHandlers({ id: state.postalCodeSecondId, value: '' });
        resetFormValidator('#main-form');
    };

    var change = function (id) {
        var item = $.grep(state.postalCodes, function (item) { return item.id === id })[0];

        return function (e) {
            e.target.value = e.target.value.toUpperCase();
            item.value = e.target.value;
            resetFormValidator('#main-form');
        }
    }

    var remove = function (e) {
        e.preventDefault();
        var id = $(this).attr('hidden-value');
        $('#postal-code-' + id).remove();
        var index = Number(id);

        var codeToDelete = $.grep(state.postalCodes, function (i) { return i.id === index })[0];

        if (state.postalCodes.indexOf(codeToDelete) !== -1) {
            state.postalCodes.splice(state.postalCodes.indexOf(codeToDelete), 1);
        }

        rebuildPostalCodeIndex(index);

        state.postalCodeSecondId--;
    }

    var checkPostalCodeCopies = function() {
        var uniqOb = {};
        var arrOfValues = state.postalCodes.map(function(obj) { return obj.value });
        for (var i in arrOfValues) {
            uniqOb[arrOfValues[i]] = "";
        }

        return state.postalCodes.length === Object.keys(uniqOb).length;
    }

    function rebuildPostalCodeIndex(id) {
        while (true) {
            id++;
            var nextPostalCode = $('div#postal-code-' + id);
            if (!nextPostalCode.length) { break; }

            var updatedState = $.grep(state.postalCodes, function (i) { return i.id === id })[0];
            if (updatedState !== undefined) {
                updatedState.id--;
            }

            nextPostalCode.attr('id', 'postal-code-' + (id - 1));
            nextPostalCode.find('a.remove-postal-code').each(function() {
                $(this).attr('id', 'remove-postal-code-' + (id - 1));
                $(this).attr('hidden-value', (id - 1));
            });

            nextPostalCode.find('input').each(function () {
                $(this).attr('id', $(this).attr('id').replace('PostalCodes_' + id, 'PostalCodes_' + (id - 1)));
                $(this).attr('name', $(this).attr('name').replace('PostalCodes[' + id, 'PostalCodes[' + (id - 1)));
            });

            nextPostalCode.find('span').each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }
                $(this).attr('data-valmsg-for', valFor.replace('PostalCodes[' + id, 'PostalCodes[' + (id - 1)));
            });

            resetFormValidator('#main-form');
        }
    }

    function resetFormValidator(formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(formId);
    }

    function setHandlers(item) {
        state.postalCodes.push(item);
        $('#PostalCodes_' + item.id + '__PostalCode').on('change', change(item.id));
        $('#remove-postal-code-' + item.id).on('click', remove);

        state.postalCodeSecondId++;
    }

    return {
        initPostalCodeState: init,
        addPostalCode: add,
        removePostalCode: remove,
        checkCopies: checkPostalCodeCopies
    }
})