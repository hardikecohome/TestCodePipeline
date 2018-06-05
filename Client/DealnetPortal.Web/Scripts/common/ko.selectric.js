ko.bindingHandlers.selectric = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var $el = $(element);
        var observable = valueAccessor();
        var options = allBindingsAccessor().options || allBindingsAccessor().foreach;
        $el.selectric({
            responsive: true
        });
        $el.on('selectric-change', function (event, element, selectric) {
            observable(element.value);
        });
        observable.subscribe(function (newValue) {
            $el.val(newValue).selectric('refresh');
        });
        if (ko.isObservable(options)) {
            var subscription = options.subscribe(function () {
                $el.val(observable());
                $el.selectric('refresh');
            });
        }
    }
};