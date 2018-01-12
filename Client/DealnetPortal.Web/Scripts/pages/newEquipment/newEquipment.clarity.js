module.exports('clarity', function() {
    
    var showLoanDetails = function() {
        $('#loanRateCardToggle').find('i.glyphicon')
            .removeClass('glyphicon-chevron-down')
            .addClass('glyphicon-chevron-up');

        $('.loan-details').removeClass('hidden');
        $('.loan-brief').addClass('hidden');
    };

    var hideLoanDetails = function() {
        $('#loanRateCardToggle').find('i.glyphicon')
            .removeClass('glyphicon-chevron-up')
            .addClass('glyphicon-chevron-down');

        $('.loan-details').addClass('hidden');
        $('.loan-brief').removeClass('hidden');
    };

    var init = function () {
        var opened = true;
        $('#loanRateCardToggle').on('click', function() {
            opened ? hideLoanDetails() : showLoanDetails();
            opened = !opened;
        });
    };

    return {
        init: init
    };
});