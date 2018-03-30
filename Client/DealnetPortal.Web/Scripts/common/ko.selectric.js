ko.bindingHandlers.selectric = {
    update: function (element, valueAccessor) {
        ko.unwrap(valueAccessor());
        $(element).selectric('refresh');
    }
};