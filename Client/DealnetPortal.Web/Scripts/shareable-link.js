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

        var enLink = $('#enLink');
        $('#copyEn').on('click', function () {
            enLink.select();
            document.execCommand('copy');
        });

        var frLink = $('#frLink');
        $('#copyFr').on('click', function () {
            frLink.select();
            document.execCommand('copy');
        });
    });