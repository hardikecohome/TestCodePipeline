module.exports('navigateToStep', function (require) {
    var dynamicAlertModal = require('alertModal').dynamicAlertModal;

    function navigateToStep(target) {
        var targetLink = $(target);
        var url = targetLink.attr('href');
        var stepName = targetLink.data('step') || translations['Edit'];
        var data = {
            message: translations['IfYouChangeInfo'],
            title: translations['NavigateToStep'] + ' ' + stepName + '?',
            confirmBtnText: translations['GoToStep'] + ' ' + stepName
        };
        dynamicAlertModal(data);

        $('#confirmAlert').on('click', function () {
            window.location.href = url;
        });
    }
    return navigateToStep;
});