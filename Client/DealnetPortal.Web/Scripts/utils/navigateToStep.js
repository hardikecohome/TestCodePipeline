module.exports('navigateToStep', function () {
    return function navigateToStep (targetLink) {
        var url = targetLink.attr('href');
        var stepName = targetLink.text() === 'Edit' ? '1' : targetLink.text();
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
});
