module.exports('newEquipment.clarity.ui', function (require) {
    var settings = {
        loanRateCardToggleId: '#loanRateCardToggle',
        loanDetailsClass: '.loan-details',
        loanBriefClass: '.loan-brief',
        addEquipmentId: '#addEquipment',
        addInstallPackageId: '#addInstallationPackage'
    }

    var setEqualHeightRows = require('setEqualHeightRows');

    var init = function () {
        _initHandlers();

        setLabelHeigths();
    };

    function setLabelHeigths() {
        setEqualHeightRows($('#new-equipments .new-equipment-equal-height-label'));
        setEqualHeightRows($('#installation-packages .installation-equal-height'));
    }

    function _initHandlers() {
        var opened = true;

        $(settings.loanRateCardToggleId).on('click', function () {
            opened ? _hideLoanDetails() : _showLoanDetails();
            opened = !opened;
        });

        $(settings.addEquipmentId).on('click', setLabelHeigths);
        $(settings.addInstallPackageId).on('click', setLabelHeigths);

        $(window).resize(setLabelHeigths);
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