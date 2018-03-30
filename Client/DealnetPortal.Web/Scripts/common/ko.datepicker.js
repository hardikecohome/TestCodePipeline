ko.bindingHandlers.datePicker = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var assignDatepicker = module.require('datepicker').assignDatepicker;
        var observable = valueAccessor();
        var options = Object.assign({}, allBindingsAccessor().datePickerOptions || {}, {
            onSelect: function (day) {
                observable(day);
            }
        });
        assignDatepicker(element, options);
    }
}