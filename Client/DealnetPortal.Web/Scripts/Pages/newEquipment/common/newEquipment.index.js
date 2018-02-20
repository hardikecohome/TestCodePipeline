module.exports('newEquipment.index', function(require) {
    //var rateCardBlock = require('rate-cards-ui');
    var navigateToStep = require('navigateToStep');
    var datepicker = require('datepicker');
    var state = require('state').state;

    var settings = Object.freeze({
        agreementTypeId: '#typeOfAgreementSelect',
        equipmentValidationMessageId: '#new-equipment-validation-message',
        addEquipmentId: '#addEquipment',
        addExistingEquipmentId: '#addExistingEqipment',
        isQuebecProvinceId: '#IsQuebecProvince',
        applicationType: {
            'loanApplication': '0',
            'rentalApplicationHwt': '1',
            'rentalApplication': '2'
        }
    });

    /**
        * Entry Point
        * @param {number} id - contract id 
        * @param {Object<>} cards - list of available rate cards for the dealer 
        * @param {boolean} onlyCustomRateCard - flag indicates that we have only one card 
        * @returns {void} 
    */
    var init = function() {
        var isOnlyLoan = $(settings.isQuebecProvinceId).val().toLowerCase() === 'true';

        if (isOnlyLoan) {
            if ($(settings.agreementTypeId).find(":selected").val() !== settings.applicationType.loanApplication) {
                $(settings.agreementTypeId).val(settings.applicationType.loanApplication);
            }
            $(settings.agreementTypeId).attr('disabled', true);
        }

        var agreementType = $(settings.agreementTypeId).find(":selected").val();
        state.agreementType = Number(agreementType);

        _initDatepickers();

        //equipment.init();
        //rateCardBlock.init();

        $('#steps .step-item[data-warning="true"]').on('click', function () {
            if ($(this).attr('href')) {
                navigateToStep($(this));
            }
            return false;
        });
    };

    function _initDatepickers () {
        var datepickerOptions = {
            yearRange: '1900:2200',
            minDate: new Date()
        };

        $('.date-input').each(function (index, input) {
            datepicker.assignDatepicker(input, datepickerOptions);
        });

        $.validator.addMethod("date",
            function (value, element) {
                var minDate = new Date("1900-01-01");
                var valueEntered = Date.parseExact(value, "M/d/yyyy");
                if (!valueEntered) {
                    return false;
                }
                if (valueEntered < minDate) {
                    return false;
                }
                return true;
            },
            translations['EnterValidDate']
        );
    }

    return {init : init}
})