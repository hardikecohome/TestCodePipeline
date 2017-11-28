module.exports('dob-selecters', function () {

    $(function () {
        $('.dob-group').each(function (i, el) {
            initDobGroup(el);
        });
    });

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

    function setInputValue (day, month, year, $input) {
        var date = new Date(year, month - 1, day);
        if (!isNaN(date.getTime())) {
            $input.val(month + '/' + day + '/' + year);
        }
    }
    function initDobGroup (el) {
        var $el = $(el);
        var $input = $el.find('.dob-input');
        var $day = $el.find('.dob-day');
        var $month = $el.find('.dob-month');
        var $year = $el.find('.dob-year');
        resetDay($day, $month, $year);
        $input.on('change', function () {
            var dateTime = new Date($input.val());
            if (isNaN(dateTime.getTime())) {
                $day.val('');
                $month.val('');
                $year.val('');
            } else {
                $day.val(dateTime.getDate());
                $month.val(dateTime.getMonth() + 1);
                $year.val(dateTime.getFullYear());
            }
        });
        $day.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day.val(), $month.val(), $year.val(), $input);
        });
        $month.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day.val(), $month.val(), $year.val(), $input);
        });
        $year.on('change', function () {
            resetDay($day, $month, $year);
            setInputValue($day.val(), $month.val(), $year.val(), $input);
        });
    }

    return initDobGroup;
});
