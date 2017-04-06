$(document)
    .ready(function() {
        var EN = 'EN';
        var FR = 'FR';

        var mapLangToSelectedOptions = {};
        mapLangToSelectedOptions[EN] = {
            list: $('#englishTagsList'),
            name: 'EnglishServices'
        };

        mapLangToSelectedOptions[FR] = {
            list: $('#frenchTagsList'),
            name: 'FrenchServices'
        };

        var activeLang = EN;

        // action handlers
        var input = $('#enEquipmentTags');
        var addBtn = $('#addEquipment');

        addBtn.on('click', function () {
            var value = input.val();
            if (value) {
                var options = mapLangToSelectedOptions[activeLang];
                options.list.append($('<li><input class="hidden" name="' + options.name + '" value="' + value + '">' + value + ' <span class="icon-remove" onclick="$(this).parent().remove()"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span></li>'));
                input.val('').placeholder();
            }
        });

        $('#englishBtn').on('click', function () {
            activeLang = EN;
        });

        $('#frenchBtn').on('click', function () {
            activeLang = FR;
        });

        $('#saveBtn').on('click', function () {
            showLoader();
            $('#mainForm').ajaxSubmit({
                type: "POST",
                success: function (json) {
                    hideLoader();
                    if (json.isError) {
                        $('.success-message').hide();
                        alert(translations['ErrorWhileUpdatingData']);
                    } else if (json.isSuccess) {
                        if ($('.toggle-language-link:checked').length) {
                            $('#success-message-disabled').hide();
                            $('#success-message-enabled').show();
                        } else {
                            $('#success-message-enabled').hide();
                            $('#success-message-disabled').show();
                        }
                    }
                },
                error: function (xhr, status, p3) {
                    hideLoader();
                    $('.success-message').hide();
                    alert(translations['ErrorWhileUpdatingData']);
                }
            });
        });

        var isIOS = navigator.userAgent.match(/ipad|ipod|iphone/i);

        var selectElement = function(el) {
            if (el.nodeName == "TEXTAREA" || el.nodeName == "INPUT")
                el.select();
            if (el.setSelectionRange && isIOS)
                el.setSelectionRange(0, 999999);
        };

        var copyCommand = function() {
            if (document.queryCommandSupported("copy")) {
                document.execCommand('copy');
            }
        };

        var enLink = document.getElementById('enLink');
        var frLink = document.getElementById('frLink');

        var activeLink = '';
        document.addEventListener('copy', function (e) {
            e.clipboardData.setData('text/plain', activeLink);
            e.preventDefault();
        });

        $('#copyEn').on('click', function () {
            activeLink = enLink.value;
            selectElement(enLink);
            copyCommand();
        });

        $('#copyFr').on('click', function () {
            activeLink = frLink.value;
            selectElement(frLink);
            copyCommand();
        });
    });