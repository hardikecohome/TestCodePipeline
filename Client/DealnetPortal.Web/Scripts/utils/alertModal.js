module.exports('alertModal', function () {

    function dynamicAlertModal (obj) {
        var classes = obj.class ? obj.class : '';
        var alertModal = $('#alertModal');
        alertModal.find('.modal-body p').html(obj.message);
        alertModal.find('.modal-title').html(obj.title ? obj.title : '');
        alertModal.find('#confirmAlert').html(obj.confirmBtnText);
        alertModal.find('.modal-footer button[data-dismiss="modal"]').html(obj.cancelBtnText);
        alertModal.addClass(classes);
        alertModal.modal('show');
        alertModal.one('hidden.bs.modal', function () {
            $('#confirmAlert').off('click');
        });
    }

    function hideDynamicAlertModal () {
        $('#alertModal').modal('hide');
        $('#confirmAlert').off('click');
    }

    return {
        dynamicAlertModal: dynamicAlertModal,
        hideDynamicAlertModal: hideDynamicAlertModal
    }
});
