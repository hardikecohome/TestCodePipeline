$(document)
    .ready(function () {
        var EN = 'EN';
        var FR = 'FR';

        var mockSources = [{ // TODO, fetch the list from backend
            value: 'Air Conditioner',
            id: 'conditioner',
        }, {
            value: 'Air Handler',
            id: 'handler',
        }];

        var selectedEnEquipments = [];
        var selectedFrEquipments = [];

        var mapLangToSelectedOptions = {
            [EN]: selectedEnEquipments,
            [FR]: selectedFrEquipments,
        };

        var activeLang = EN;

        var onDeleteEquipment = function (id) {
            return function () {
                var selectedEquipments = mapLangToSelectedOptions[activeLang];

                var index = selectedEquipments.findIndex(function (item) { return id === item.id; });
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
                    span.on('click', onDeleteEquipment(item.id));
                    return $("<li></li>").append(item.value).append(span);
                }));
        };


        // equipments autocomplete
        var input = $('#enEquipmentTags');
        var closeBtn = $('#equipmentContainer').find('a');
        var EmptyEquipment = { id: '@@EmptyEquipment', value: '@@EmptyValue' };
        var selectedEquipment = EmptyEquipment;
        
        input.autocomplete({
            appendTo: '#equipmentContainer',
            source: mockSources,
            classes: {
                'ui-autocomplete': 'dropdown-menu',
            },
            messages: {
                noResults: '',
                results: function () { },
            },
            select: function (event, ui) {
                var item = ui.item;
                var selectedEquipments = mapLangToSelectedOptions[activeLang];

                if (!selectedEquipments.some(function (el) { return item.id === el.id; })) {
                    selectedEquipment = item;
                } else {
                    selectedEquipment = EmptyEquipment;
                }
            },
            minLength: 0,
        }).data('ui-autocomplete')._renderItem = function (ul, item) {
            var link = $("<a></a>").append(item.label);

            return $("<li></li>")
                .append(link)
                .appendTo(ul);
        };

        // action handlers
        var addBtn = $('#addEquipment');

        addBtn.on('click', function () {
            var value = input.val();
            if (value && value === selectedEquipment.value) {
                var selectedEquipments = mapLangToSelectedOptions[activeLang];
                selectedEquipments.push(selectedEquipment);
                renderEquipments(selectedEquipments);
                input.val('');
                closeBtn.hide();
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