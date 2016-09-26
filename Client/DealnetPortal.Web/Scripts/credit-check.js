var oldParent = null;


$(document)
    .ready(function() {
        $('#editor-modal')
            .on('show.bs.modal', function(){
              toggleClearInputIcon($('.modal-body input, textarea'));
            })
            .on('hidden.bs.modal',
                function() {
                    $('.modal-body .dealnet-credit-check-section').
                        detach().appendTo(oldParent);
                    $('.dealnet-credit-check-section input').removeClass('form-control dealnet-input');
                    $('.dealnet-credit-check-section').removeClass('dealnet-modal-section');
                    $('.dealnet-section-title').show();
                    $('.dealnet-credit-check-section a').show();
                    $('input[type="text"]').attr('readonly', 'readonly');
                    $('input[type="text"]').addClass('dealnet-disabled-input');
                    $('.dealnet-agrees').show();
                });
        $('input[type="text"]').attr('readonly', 'readonly');
        $('input[type="text"]').addClass('dealnet-disabled-input');

        $("#birth-date").datepicker({
            dateFormat: 'mm/dd/yy',
            changeMonth: true,
            changeYear: true,
            yearRange: '1900:2016',
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date()
        });
        $("#additional-birth-date-1").datepicker({
            dateFormat: 'mm/dd/yy',
            changeMonth: true,
            changeYear: true,
            yearRange: '1900:2016',
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date()
        });
        $("#additional-birth-date-2").datepicker({
            dateFormat: 'mm/dd/yy',
            changeMonth: true,
            changeYear: true,
            yearRange: '1900:2016',
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date()
        });
        $("#additional-birth-date-3").datepicker({
            dateFormat: 'mm/dd/yy',
            changeMonth: true,
            changeYear: true,
            yearRange: '1900:2016',
            minDate: Date.parse("1900-01-01"),
            maxDate: new Date()
        });
        //
        $.validator.addMethod(
            "date",
            function(value, element) {
                var minDate = Date.parse("1900-01-01");
                var maxDate = new Date();
                var valueEntered = Date.parseExact(value, "M/d/yyyy");
                if (!valueEntered) {
                    return false;
                }
                if (valueEntered < minDate || valueEntered > maxDate) {
                    return false;
                }
                return true;
            },
            "Please enter a valid date!"
        );
    });






function editData(elem) {
    var section = $(elem).parents('.dealnet-credit-check-section');
    oldParent = section.parent();
    section.detach().appendTo('.modal-body');
    section.find('input').addClass('form-control dealnet-input');
    $('.modal-title').text(section.find('.dealnet-section-title').text());
    section.find('.dealnet-section-title').hide();
    section.find('input[type="text"]').removeAttr('readonly');
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


function saveChanges(url) {
    $('#credit-check-form').validate();
    if ($('#credit-check-form').valid()) {
        $('#editor-modal').modal('hide');
        submitChanges(url);
    };
};


function cancelChanges() {
    $('.modal-body input[type="text"]').each(function (index, elem) {
        $(elem).val($(elem).attr('default-value'));
    });
    $('.text-danger span').remove();
    $('#editor-modal').modal('hide');
    

};

function submitChanges(url) {
    showLoader();
    $('#credit-check-form').ajaxSubmit({
        type: "POST",
        url: url,
        success: function (json) {
            hideLoader();
            if (json.isError) {
                alert("An error occurred while updating data");
            }
        },
        error: function (xhr, status, p3) {
            hideLoader();
            alert(xhr.responseText);
        }
    });
}