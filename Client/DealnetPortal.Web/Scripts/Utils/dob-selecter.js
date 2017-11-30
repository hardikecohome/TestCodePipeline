module.exports('dob-selecters', function () {
    var today = new Date();
    var todayLess18 = new Date(today.getFullYear() - 18, today.getMonth(), today.getDate());

    function initDobGroup (el) {
        var $el = $(el);
        var $input = $el.find('.dob-input');
        var $day = $el.find('.dob-day');
        var $month = $el.find('.dob-month');
        var $year = $el.find('.dob-year');
        resetDay($day, $month, $year);
        $input.on('change', function () {
            var dateTime = new Date($input.val());
            if (isNaN(dateTime.getTime()) || dateTime > todayLess18) {
                $input.val('');
            } else {
                $day.val(dateTime.getDate());
                $month.val(dateTime.getMonth() + 1);
                $year.val(dateTime.getFullYear());
            }
        });
        $day.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day, $month, $year, $input);
        });
        $month.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day, $month, $year, $input);
        });
        $year.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day, $month, $year, $input);
        });
    }

    function clearDate ($day) {
        $day.val('').change();
    }

    function resetDay ($day, $month, $year) {
        $day.find('option').prop('disabled', false).removeClass('hidden');
        var month = $month.val();
        var day = $day.val();
        var year = $year.val();
        if (month == 4 || month == 6 || month == 9 || month == 11) {
            if (day == 31) clearDate($day);
            $day.find('option[value="31"]').prop('disabled', true).addClass('hidden');
        } else if (month == 2) {
            if (year % 4 != 0) {
                if (day == 29) clearDate($day);;
                $day.find('option[value="29"]').prop('disabled', true).addClass('hidden');
            }
            if (day == 30 || day == 31) clearDate($day);;
            $day.find('option[value="30"]').prop('disabled', true).addClass('hidden');
            $day.find('option[value="31"]').prop('disabled', true).addClass('hidden');
        }
    }

    function setInputValue ($day, $month, $year, $input) {
        var day = $day.val();
        var month = $month.val();
        var year = $year.val();
        if (day && month && year) {
            $input.val(month + '/' + day + '/' + year).change();
            if (!$input.valid()) {
                $day.addClass('input-validation-error');
                $month.addClass('input-validation-error');
                $year.addClass('input-validation-error');
            } else {
                $day.removeClass('input-validation-error');
                $month.removeClass('input-validation-error');
                $year.removeClass('input-validation-error');
            }

        } else {
            $input.val('').change();
            $day.addClass('input-validation-error');
            $month.addClass('input-validation-error');
            $year.addClass('input-validation-error');
        }
    }

    function validateInput (input) {
        var $input = $(input);
        var $group = $input.parents('.dob-group');
        if (!$input.valid()) {
            $group.find('select').addClass('input-validation-error');
        } else {
            $group.find('select').removeClass('input-validation-error');
        }
    }

    return {
        initDobGroup: initDobGroup,
        validate: validateInput
    };
});
