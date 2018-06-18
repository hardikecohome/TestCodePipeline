ko.bindingHandlers.selectric = {
    init: function (element, valueAccessor, allBindingsAccessor) {
        var $el = $(element);
        var observable = valueAccessor();
        var options = allBindingsAccessor().options || allBindingsAccessor().foreach;
        $el.selectric({
            responsive: true,
            nativeOnMobile: false,
            onRefresh: function (element, selectric) {
                appendNotSelected(selectric);
            }
        });

        var $label = $el.parents('.custom-select-wrap.custom-select-float-label').find('label');

        $el.on('selectric-before-open', function () {
            $label.addClass('label-title');
        });

        $el.on('selectric-close', function () {
            !$el.val() && $label.removeClass('label-title');
        });

        $el.on('selectric-change', function (event, element) {
            observable(element.value);
            $el.val() ?
                $label.addClass('label-title') :
                $label.removeClass('label-title');
        });

        $label.on('click', function () {
            $el.selectric('open');
        });

        if ($el.val()) {
            $label.addClass('label-title');
        }

        appendNotSelected($el.data('selectric'));

        observable.subscribe(function (newValue) {
            $el.val(newValue).selectric('refresh');
            newValue ?
                $label.addClass('label-title') :
                $label.removeClass('label-title');
        });

        if (ko.isObservable(options)) {
            var subscription = options.subscribe(function (newValue) {
                $el.val(observable());
                $el.selectric('refresh');
            });
        }
    }
};

function appendNotSelected(sel) {
    var emptyLi = sel.elements.items.find('li:empty:not(.selectric-group-label)');
    if (emptyLi.length) {
        emptyLi.text(translations.NotSelected);
        emptyLi.addClass('not-selected');
    }
}