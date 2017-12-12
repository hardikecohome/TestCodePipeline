module.exports('panelCollapsed', function () {
    return function panelCollapsed (elem) {
        var $this = elem.closest('.panel');
        $this.find('.panel-body').slideToggle('fast', function () {
            $this.toggleClass('panel-collapsed');
        });
    };
});