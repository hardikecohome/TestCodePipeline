module.exports('postalCode-handlers', function (require) {
    var template = require('my-profile-template');
    var state = require('my-profile-state');

    var change = function (id) {
        console.log('set id ' + id);
        var item = $.grep(state.postalCodes, function (item) { return item.id === id })[0];

        return function (e) {
            item.value = e.target.value;
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

        while (true) {
            index++;
            var nextPostalCode = $('div#postal-code-' + index);
            if (!nextPostalCode.length) { break; }

            var updatedState = $.grep(state.postalCodes, function (i) { return i.id === index })[0];
            updatedState.id--;

            var inputs = nextPostalCode.find('input');

            inputs.each(function () {
                $(this).attr('id', $(this).attr('id').replace('PostalCodes_' + index, 'PostalCodes_' + (index - 1)));
                $(this).attr('name', $(this).attr('name').replace('PostalCodes[' + index, 'PostalCodes[' + (index - 1)));
            });

            var spans = nextPostalCode.find('span');
            spans.each(function () {
                var valFor = $(this).attr('data-valmsg-for');
                if (valFor == null) { return; }
                $(this).attr('data-valmsg-for', valFor.replace('PostalCodes[' + index, 'PostalCodes[' + (index - 1)));
            });
            resetFormValidator('#main-form');
        }

        state.postalCodeSecondId--;
    }

    function resetFormValidator(formId) {
        $(formId).removeData('validator');
        $(formId).removeData('unobtrusiveValidation');
        $.validator.unobtrusive.parse(formId);
    }

    function setHandlers(item) {
        state.postalCodes.push(item);
        $('#PostalCodes_' + item.id + '__Value').on('change', change(item.id));
        $('#remove-postal-code-' + item.id).on('click', remove);

        state.postalCodeSecondId++;
    }

    var init = function () {
        var postalCodes = $('div#postal-code-area').find('[id^=postal-code-]');
        for (var i = 0; i < postalCodes.length; i++) {
            var value = $(postalCodes[i]).find('#PostalCodes_' + i + '__Value').val();
            setHandlers({ id: i, value: value });
        }
    }

    var add = function() {
        var newTemplate = template($('<div></div>'), { id: state.postalCodeSecondId });
        state.postalCodes.push({ id: state.postalCodeSecondId, value: '' });

        $('#postal-code-area').append(newTemplate);
        setHandlers({ id: state.postalCodeSecondId, value: '' });
        resetFormValidator('#main-form');
    };

    return {
        initPostalCodeState: init,
        addPostalCode: add,
        removePostalCode: remove
    }
})