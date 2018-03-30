ko.bindingHandlers.datePicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var assignDatepicker = module.require('datepicker').assignDatepicker;
        var observable = valueAccessor();
        var options = Object.assign({}, allBindingsAccessor().datePickerOptions || {}, {
            onSelect: function (day) {
                observable(day);
            }
        });
        var input = assignDatepicker(element, options);

        observable.subscribe(function (newValue) {
            input.val(newValue);
        });
    }
};