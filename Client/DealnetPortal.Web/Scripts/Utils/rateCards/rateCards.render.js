module.exports('rateCards.render', function (require) {
    var settings = {
        numberFields: ['equipmentSum', 'LoanTerm', 'AmortizationTerm', 'CustomerRate', 'DealerCost', 'AdminFee'],
        notCero: ['equipmentSum', 'LoanTerm', 'AmortizationTerm']
    };

    var init = function(viewSettings) {
        $.extend(settings, viewSettings);
    }

    var render = function(option, data) {
        if (_isEmpty(settings))
            throw new Error('settings are empty. Use init method first.');

        var notNan = !Object.keys(data).map(_idToValue(data)).some(function (val) { return isNaN(val); });
        var validateNumber = settings.numberFields.every(function (field) { return typeof data[field] === 'number'; });
        var validateNotEmpty = settings.notCero.every(function (field) { return data[field] !== 0; });

        if (notNan && validateNumber && validateNotEmpty) {
            Object.keys(settings.rateCardFields).map(function (key) { $('#' + option + key).text(formatCurrency(data[settings.rateCardFields[key]])); });
        } else {
            Object.keys(settings.rateCardFields).map(function (key) { $('#' + option + key).text('-'); });
        }
    }

    var renderTotalPrice = function (option, data) {
        var notNan = !Object.keys(data).map(_idToValue(data)).some(function (val) { return isNaN(val); });
        if (notNan) {
            Object.keys(settings.totalPriceFields).map(function (key) { $('#' + option + key).text(formatCurrency(data[settings.totalPriceFields[key]])); });
            $('#totalEquipmentPrice').text(formatNumber(data.equipmentSum));
            $('#tax').text(formatNumber(data.tax));
            $('#totalPrice').text(formatNumber(data.totalPrice));
        } else {
            Object.keys(settings.totalPriceFields).map(function (key) { $('#' + option + key).text(formatCurrency(data[settings.totalPriceFields[key]])); });
            $('#totalEquipmentPrice').text('-');
            $('#tax').text('-');
            $('#totalPrice').text('-');
        }
    };

    var renderDropdownValues = function(items) {
        
    }

    var renderAfterFiltration = function(option) {

        rateCardBlock.togglePromoLabel(option.name);

        if (option.name === 'Deferral') {
            $('#DeferralPeriodDropdown').val(state[option.name].DeferralPeriod);
        }

        $('#' + option.name + 'AFee').text(formatCurrency(state[option.name].AdminFee));
        $('#' + option.name + 'CRate').text(formatNumber(state[option.name].CustomerRate) + ' %');
        $('#' + option.name + 'YCostVal').text(formatNumber(state[option.name].DealerCost) + ' %');
    }

    function _isEmpty(obj) {
        if (obj === null) return true;

        for (var key in obj) {
            if (obj.hasOwnProperty(key)) {
                return false;
            }
        }

        return Object.key(obj).length === 0;
    }

    function _idToValue(obj) {
        return function (id) {
            return obj.hasOwnProperty(id) ? obj[id] : '';
        };
    };

    return {
        init: init,
        render: render,
        renderTotalPrice: renderTotalPrice
    };
});