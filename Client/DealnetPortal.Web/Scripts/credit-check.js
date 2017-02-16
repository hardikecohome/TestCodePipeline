$(document)
    .ready(function() {
        $("#check-credit-button").click(function (event) {
            var check1 = document.getElementById('home-owner-agrees');
            var check2 = document.getElementById('additional1-agrees');
            var check3 = document.getElementById('additional2-agrees');
            var check4 = document.getElementById('additional3-agrees');
            if (!(check1.checked &&
            (check2 == null || check2.checked) &&
            (check3 == null || check3.checked) &&
            (check4 == null || check4.checked))) {
                event.preventDefault();
                $("#proceed-error-message").show();
            }
        });
    });