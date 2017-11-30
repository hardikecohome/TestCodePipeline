﻿module.exports('dob-selecters', function () {
    function initDobGroup (el) {
        var $el = $(el);
        var $input = $el.find('.dob-input');
        var $day = $el.find('.dob-day');
        var $month = $el.find('.dob-month');
        var $year = $el.find('.dob-year');
        resetDay($day, $month, $year);

        $day.rules('add', {
            required: true,
            messages: {
                required: translations['ThisFieldIsRequired']
            }
        });
        $month.rules('add', {
            required: true,
            messages: {
                required: translations['ThisFieldIsRequired']
            }
        });
        $year.rules('add', {
            required: true,
            messages: {
                required: translations['ThisFieldIsRequired']
            }
        });
        $input.on('change', function () {
            var dateTime = new Date($input.val());
            if (isNaN(dateTime.getTime())) {
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
            $day.valid();
        });
        $month.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day, $month, $year, $input);
            $month.valid();
        });
        $year.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day, $month, $year, $input);
            $year.valid();
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
        } else {
            $input.val('').change();
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
