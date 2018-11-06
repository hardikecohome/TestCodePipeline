$(document)
    .ready(function () {
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
        var addBtn = $('#addEquipment, #addEquipmentXS');

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
            module.require('loader').showLoader();
            $('#mainForm').ajaxSubmit({
                type: "POST",
                success: function (json) {
                    module.require('loader').hideLoader();
                    if (json.isError) {
                        $('.success-message').hide();
                        alert(translations['ErrorWhileUpdatingData']);
                    } else if (json.isSuccess) {
                        if ($('.toggle-language-link:checked').length) {
                            $('#success-message-disabled').addClass('hidden');
                            $('#success-message-enabled').removeClass('hidden');
                        } else {
                            $('#success-message-enabled').addClass('hidden');
                            $('#success-message-disabled').removeClass('hidden');
                        }
                    }
                },
                error: function (xhr, status, p3) {
                    module.require('loader').hideLoader();
                    $('.success-message').hide();
                    alert(translations['ErrorWhileUpdatingData']);
                }
            });
        });

        var isIOS = navigator.userAgent.match(/ipad|ipod|iphone/i);

        var selectElement = function (el) {
            if (el.nodeName === "TEXTAREA" || el.nodeName === "INPUT")
                el.select();
            if (el.setSelectionRange && isIOS)
                el.setSelectionRange(0, 999999);
        };

        var copyCommand = function () {
            if (document.queryCommandSupported("copy")) {
                document.execCommand('copy');
            }
        };

        var enLink = document.getElementById('enLink');
        var frLink = document.getElementById('frLink');
        if (!isIOS) {

            var activeLink = '';

            $('#copyEn').on('click',
                function () {
                    activeLink = enLink.value;
                    selectElement(enLink);
                    copyCommand();
                });

            $('#copyFr').on('click',
                function () {
                    activeLink = frLink.value;
                    selectElement(frLink);
                    copyCommand();
                });
        } else {
            enLink = $('#enLink');
            frLink = $('#frLink');

            var enLinkVal = enLink.val();
            var frLinkVal = frLink.val();

            enLink.parent().append($.parseHTML('<a href="' + enLinkVal + '" style="word-wrap: break-word;">' + enLinkVal + '</a>'));
            frLink.parent().append($.parseHTML('<a href="' + frLinkVal + '" style="word-wrap: break-word;">' + frLinkVal + '</a>'));

            enLink.hide();
            frLink.hide();
            $('#copyEn').hide();
            $('#copyFr').hide();
        }
    });