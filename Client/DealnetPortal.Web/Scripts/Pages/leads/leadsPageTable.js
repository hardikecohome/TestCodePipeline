module.exports('table', function(require) {

    var Paginator = require('paginator');

    var LeadsTable = function(list) {
        this.datePickerOptions = {
            yearRange: '1900:' + new Date().getFullYear(),
            minDate: new Date("1900-01-01"),
            maxDate: new Date(),
        };
    }
})