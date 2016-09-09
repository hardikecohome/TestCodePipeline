var oldParent = null;


$(document)
    .ready(function () {
        $('#editor-modal')
            .on('hidden.bs.modal',
                function () {
                    $('.modal-body .dealnet-credit-check-section').
                        detach().appendTo(oldParent);
                    $('.dealnet-credit-check-section input').removeClass('form-control dealnet-input');
                    $('.dealnet-credit-check-section').removeClass('dealnet-modal-section');
                    $('.dealnet-section-title').show();
                    $('.dealnet-credit-check-section a').show();
                    $('input[type="text"]').attr('disabled','disabled');
                    $('input[type="text"]').addClass('dealnet-disabled-input');
                    $('.dealnet-agrees').show();
                });
        $('input[type="text"]').attr('disabled', 'disabled');
        $('input[type="text"]').addClass('dealnet-disabled-input');
        
        $("#birth-date").datepicker({
            dateFormat: 'mm/dd/yy', changeMonth: true,
            changeYear: true
        });
        $("#additional-birth-date-1").datepicker({
            dateFormat: 'mm/dd/yy', changeMonth: true,
            changeYear: true
        });
        $("#additional-birth-date-2").datepicker({
            dateFormat: 'mm/dd/yy', changeMonth: true,
            changeYear: true
        });
        $("#additional-birth-date-3").datepicker({
            dateFormat: 'mm/dd/yy', changeMonth: true,
            changeYear: true
        });
    });






function editData(elem) {
    var section = $(elem).parents('.dealnet-credit-check-section');
    oldParent = section.parent();
    section.detach().appendTo('.modal-body');
    section.find('input').addClass('form-control dealnet-input');
    $('.modal-title').text(section.find('.dealnet-section-title').text());
    section.find('.dealnet-section-title').hide();
    section.find('input[type="text"]').removeAttr('disabled');
    section.find('input[type="text"]').removeClass('dealnet-disabled-input');
    section.addClass('dealnet-modal-section');
    section.find('a').hide();
    section.find('.dealnet-agrees').hide();
    section.find('input[type="text"]').each(function(index, elem) {
        $(elem).attr('default-value', $(elem).val());
    });
   
    $('.dealnet-credit-check-section a').hide();
    $('#editor-modal').modal();
};


function saveChanges() {
    $('#credit-check-form').validate();
    if ($('#credit-check-form').valid()) {
        $('#editor-modal').modal('hide');
    };
};


function cancelChanges() {
    $('.modal-body input[type="text"]').each(function (index, elem) {
        $(elem).val($(elem).attr('default-value'));
    });
    $('.text-danger span').remove();
    $('#editor-modal').modal('hide');
    

};