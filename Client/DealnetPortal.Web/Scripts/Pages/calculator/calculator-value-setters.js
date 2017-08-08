module.exports('calculator-value-setters', function (require) {
    var state = require('calculator-state').state;
    var constants = require('calculator-state').constants;

    var notNaN = function (num) { return !isNaN(num); };

    var idToValue = function (obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };
    
    var equipmentSum = function (equipments) {
        return Object.keys(equipments)
            .map(idToValue(equipments))
            .map(function (equipment) { return parseFloat(equipment.cost); })
            .filter(notNaN)
            .reduce(function (sum, cost) { return sum + cost; }, 0);
    };

    var setLoanAmortTerm = function (optionKey, callback) {
        return function (e) {
            callback([optionKey]);
        };
    };

    var setLoanTerm = function (optionKey, callback) {
        return function (e) {
            state[optionKey].LoanTerm = parseFloat(e.target.value);
            validateLoanAmortTerm(optionKey);
            callback([ optionKey ]);
        };
    };

    var setAmortTerm = function (optionKey, callback) {
        return function (e) {
            state[optionKey].AmortizationTerm = parseFloat(e.target.value);
            validateLoanAmortTerm(optionKey);
            callback([optionKey]);
        };
    };

    var setDeferralPeriod = function (optionKey, callback) {
        return function (e) {
            state[optionKey].DeferralPeriod = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setCustomerRate = function (optionKey, callback) {
        return function (e) {
            state[optionKey].CustomerRate = parseFloat(e.target.value);
            callback([ optionKey ]);
        };
    };

    var setYourCost = function (optionKey, callback) {
        return function (e) {
            state[optionKey].DealerCost = parseFloat(e.target.value);
            callback([optionKey]);
        };
    };

    var setAdminFee = function(optionKey, callback) {
        return function (e) {
            state[optionKey].AdminFee = Number(e.target.value);
            callback([optionKey]);
        };
    };

    var setDownPayment = function(optionKey, callback) {
        return function(e) {
            state[optionKey].downPayment = e.target.value !== '' ?  parseFloat(e.target.value) : 0;
            callback([optionKey]);
        }
    };

    var setRateCardPlan = function(optionKey, callback) {
        return function (e) {
            var planType = $.grep(constants.rateCards, function (c) { return c.name === e.target.value; })[0].id;
            state[optionKey].plan = planType;

            setAmortizationDropdownValues(optionKey, planType);

            var dropdownParentDiv = $('#' + optionKey + '-deferralDropdownWrapper');

            if (planType === 2 || planType === 3) {
                if (dropdownParentDiv.is(':hidden')) {
                    $('#' + optionKey + '-deferral').addClass('hidden');
                    dropdownParentDiv.removeClass('hidden');
                    $('#' + optionKey + '-deferralDropdown').change();
                }

                if (planType === 3) {
                    $('#' + optionKey + '-amortDropdown').closest('.row').addClass('hidden');
                    var defDropdown = $('#' + optionKey + '-deferralDropdown');
                    if(defDropdown.find('option[value=0]').length === 0)
                        defDropdown.prepend("<option value='0' selected='selected'>No Deferral</option>");
                    $.grep(constants.inputsToHide,
                        function(field) {
                            $('#' + optionKey + field).addClass('hidden');
                        });

                    $.grep(constants.customInputsToShow,
                        function(field) {
                            $('#' + optionKey + field).removeClass('hidden').attr('disabled', false);
                        });
                } else {
                    $('#' + optionKey + '-deferralDropdown option[value="0"]').remove();
                    //$('#' + optionKey + '-downPayment').val('');
                    $('#' + optionKey + '-customLoanTerm').val('');
                    $('#' + optionKey + '-customAmortTerm').val('');
                    $('#' + optionKey + '-customCRate').val('');
                    $('#' + optionKey + '-customYCostVal').val('');
                    $('#' + optionKey + '-customAFee').val('');

                    $('#' + optionKey + '-amortDropdown').closest('.row').removeClass('hidden');
                    $.grep(constants.inputsToHide, function (field) {
                        $('#' + optionKey + field).removeClass('hidden');
                    });

                    $.grep(constants.customInputsToShow, function (field) {
                        $('#' + optionKey + field).addClass('hidden');
                        $('#' + optionKey + field).siblings('a.clear-input').css('display', 'none');
                        if (field !== '-deferralDropdown') {
                            $('#' + optionKey + field).attr('disabled', true).val('');
                        }
                    });
                    dropdownParentDiv.removeClass('hidden');
                }

            } else {
                $('#' + optionKey + '-deferral').removeClass('hidden');
                //$('#' + optionKey + '-downPayment').val('');
                $('#' + optionKey + '-customLoanTerm').val('');
                $('#' + optionKey + '-customAmortTerm').val('');

                if (dropdownParentDiv.is(':visible')) {
                    dropdownParentDiv.addClass('hidden');
                }

                $('#' + optionKey + '-amortDropdown').closest('.row').removeClass('hidden');
                $.grep(constants.inputsToHide, function (field) {
                    $('#' + optionKey + field).removeClass('hidden');
                });
                
                $.grep(constants.customInputsToShow, function (field) {
                    $('#' + optionKey + field).addClass('hidden');
                    $('#' + optionKey + field).siblings('a.clear-input').css('display', 'none');
                    if (field !== '-deferralDropdown') {
                        $('#' + optionKey + field).attr('disabled', true).val('');
                    }
                });
            }

            callback([optionKey]);
        }
    }

    var setEquipmentCost = function(optionKey, callback) {
        return function (e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Cost')[0].substr(mvcId.split('__Cost')[0].lastIndexOf('_') + 1);

            state[optionKey].equipments[id].cost = isNaN(parseFloat(e.target.value)) ? '' : parseFloat(e.target.value);
            callback([optionKey]);
        }
    }

    var setEquipmentType = function (optionKey) {
        return function (e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Type')[0].substr(mvcId.split('__Type')[0].lastIndexOf('_') + 1);
            state[optionKey].equipments[id].type = e.target.value;
        }
    }

    var setEquipmentDescription = function(optionKey) {
        return function(e) {
            var mvcId = e.target.id;
            var id = mvcId.split('__Description')[0].substr(mvcId.split('__Description')[0].lastIndexOf('_') + 1);
            state[optionKey].equipments[id].description = e.target.value;
        }
    }

    var setNewEquipment = function (optionKey, callback) {
        var nextIndex = state[optionKey].equipmentNextIndex;
        state[optionKey].equipments[nextIndex] = { id: nextIndex.toString(), cost: '', description: '', type: state.defaultEquipment };
        state[optionKey].equipmentNextIndex++;

        callback([optionKey]);
    }

    var removeEquipment = function (optionKey, id, callback) {
        $('#' + optionKey + '-equipment-' + id).remove();
        state[optionKey].equipmentNextIndex--;

        callback([optionKey]);
    }

    var setTax = function(callback) {
        return function(e) {
            var name = e.target.value;
            var filtered = state.taxes.filter(function(tax) {
                return tax.Province === name;
            });

            state.tax = filtered[0].Rate;
            state.description = filtered[0].Description;

            callback();
        }
    }

    function setAmortizationDropdownValues(optionKey, planType) {
        var options = $('#' + optionKey + '-amortDropdown');
        options.empty();
        var dropdownValues = state.amortLoanPeriods[planType];
        
        dropdownValues.forEach(function (item) {
            var optionTemplate = $("<option />").val(item).text(item);

            options.append(optionTemplate);
        });
    }

    function validateLoanAmortTerm(optionKey) {
        var amortTerm = state[optionKey].AmortizationTerm;
        var loanTerm = state[optionKey].LoanTerm;
        var error = $('#' + optionKey + '-amortLoanTermError');
        if (typeof amortTerm == 'number' && typeof loanTerm == 'number') {
            if (loanTerm > amortTerm) {
                if (error.is(':hidden')) {
                    error.show();
                    error.parent().find('input[type="text"]')
                      .addClass('input-validation-error')
                      .addClass('input-has-error');
                }
            } else {
                if (error.is(':visible')) {
                    error.hide();
                    error.parent().find('input[type="text"]')
                      .removeClass('input-validation-error')
                      .removeClass('input-has-error');
                }
            }
        }
    }

    return {
        notNaN: notNaN,
        equipmentSum: equipmentSum,
        setLoanAmortTerm: setLoanAmortTerm,
        setDeferralPeriod: setDeferralPeriod,
        setCustomerRate: setCustomerRate,
        setYourCost: setYourCost,
        setAdminFee: setAdminFee,
        setDownPayment: setDownPayment,
        setRateCardPlan: setRateCardPlan,
        setEquipmentCost: setEquipmentCost,
        setEquipmentType: setEquipmentType,
        setEquipmentDescription: setEquipmentDescription,
        setNewEquipment: setNewEquipment,
        removeEquipment: removeEquipment,
        setTax: setTax,
        setLoanTerm: setLoanTerm,
        setAmortTerm: setAmortTerm
    }
});