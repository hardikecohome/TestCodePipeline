$(document)
    .ready(function () {
        var EN = 'EN';
        var FR = 'FR';

        var selectedEnEquipments = [];
        var selectedFrEquipments = [];

        var mapLangToSelectedOptions = {
            [EN]: selectedEnEquipments,
            [FR]: selectedFrEquipments,
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
                    var span = $('<span class="glyphicon glyphicon-remove"></span>');
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
                var selectedEquipments = mapLangToSelectedOptions[activeLang];
                selectedEquipments.push(value);
                renderEquipments(selectedEquipments);
                input.val('');
            }
        });

        $('#englishBtn').on('click', function () {
            activeLang = EN;
            renderEquipments(mapLangToSelectedOptions[activeLang]);
        });

        $('#frenchBtn').on('click', function () {
            activeLang = FR;
            renderEquipments(mapLangToSelectedOptions[activeLang]);
        });

        $('#saveBtn').on('click', function () {
            // TODO: send both french and english lists to backend.
            console.log(selectedEnEquipments, selectedFrEquipments);
        });

        $('#toggleEnLink').on('change', function () {
            // TODO: send link status to backend
            console.log($(this).is(':checked'));
        });

        $('#toggleFrLink').on('change', function () {
            // TODO: send link status to backend
            console.log($(this).is(':checked'));
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