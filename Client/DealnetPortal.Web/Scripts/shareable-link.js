$(document)
    .ready(function() {
        var EN = 'EN';
        var FR = 'FR';

        var mapLangToSelectedOptions = {
            [EN]: {
                list: $('#englishTagsList'),
                name: 'EnglishServices'
            },
            [FR]: {
                list: $('#frenchTagsList'),
                name: 'FrenchServices'
            }
        };

        var activeLang = EN;

        var onDeleteEquipment = function (item) {
            return function () {
                var selectedEquipments = mapLangToSelectedOptions[activeLang];

                var index = selectedEquipments.indexOf(item);
                if (index !== -1) {
                    selectedEquipments.splice(index);
                    renderEquipments(selectedEquipments);
                }
            };
        };

        // render list
        var listElm = $('#tagsList');
        var renderEquipments = function (list) {
            listElm.empty()
                .append(list.map(function (item) {
                    var span = $('<span class="icon-remove"><svg aria-hidden="true" class="icon icon-remove-cross"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-remove-cross"></use></svg></span>');
                    span.on('click', onDeleteEquipment(item));
                    return $("<li></li>").append(item).append(span);
                }));
        };

        // action handlers
        var input = $('#enEquipmentTags');
        var addBtn = $('#addEquipment');

        addBtn.on('click', function () {
            var value = input.val();
            if (value) {
                var options = mapLangToSelectedOptions[activeLang];
                options.list.append($('<li><input class="hidden" name="' + options.name + '" value="' + value + '">' + value + ' <span class="glyphicon glyphicon-remove" click="$(this).parent().remove()"></span></li>'));
                input.val('').placeholder();
            }
        });

        $('#englishBtn').on('click', function () {
            activeLang = EN;
            //renderEquipments(mapLangToSelectedOptions[activeLang]);
        });

        $('#frenchBtn').on('click', function () {
            activeLang = FR;
            //renderEquipments(mapLangToSelectedOptions[activeLang]);
        });

        $('#saveBtn').on('click', function () {
            $('#mainForm').ajaxSubmit({
                type: "POST",
                success: function (json) {
                    alert("Success");
                },
                error: function (xhr, status, p3) {
                    alert("Error");
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