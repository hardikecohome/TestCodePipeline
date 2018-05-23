ko.bindingHandlers.datePicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var observable = valueAccessor();
        var options = Object.assign({}, allBindingsAccessor().datePickerOptions || {}, {
            onSelect: function (day) {
                observable(day);
            }
        });
        var input = $(element).datepicker(options);

        observable.subscribe(function (newValue) {
            input.val(newValue);
        });
    }
};