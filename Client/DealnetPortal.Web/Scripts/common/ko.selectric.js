ko.bindingHandlers.selectric = {
    init: function (element, valueAccessor) {
        var $el = $(element);
        var observable = valueAccessor();
        $el.selectric('init');
        $el.on('selectric-change', function (event, element, selectric) {
            observable(element.value);
        });
        observable.subscribe(function (newValue) {
            $el.val(newValue).selectric('refresh');
        }, null, 'change');
    }
};