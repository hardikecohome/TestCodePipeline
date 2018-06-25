module.exports('newEquipment.clarity.ui', function (require) {
    var settings = {
        loanRateCardToggleId: '#loanRateCardToggle',
        loanDetailsClass: '.loan-details',
        loanBriefClass: '.loan-brief'
    }

    var setEqualHeightRows = require('setEqualHeightRows');

    var init = function () {
        _initHandlers();

        $(window).resize(function () {
            setEqualHeightRows($('#new-equipments .new-equipment-equal-height-label'));
        });
        setEqualHeightRows($('#new-equipments .new-equipment-equal-height-label'));
    };

    function _initHandlers() {
        var opened = true;
        $(settings.loanRateCardToggleId).on('click', function () {
            opened ? _hideLoanDetails() : _showLoanDetails();
            opened = !opened;
        });
    }

    function _showLoanDetails() {
        $(settings.loanRateCardToggleId).find('i.glyphicon')
            .removeClass('glyphicon-chevron-down')
            .addClass('glyphicon-chevron-up');

        $(settings.loanDetailsClass).removeClass('hidden');
        $(settings.loanBriefClass).addClass('hidden');
    };

    function _hideLoanDetails() {
        $(settings.loanRateCardToggleId).find('i.glyphicon')
            .removeClass('glyphicon-chevron-up')
            .addClass('glyphicon-chevron-down');

        $(settings.loanDetailsClass).addClass('hidden');
        $(settings.loanBriefClass).removeClass('hidden');
    };

    return {
        init: init
    };
});