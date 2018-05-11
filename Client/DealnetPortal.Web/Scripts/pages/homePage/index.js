module.exports('home-page-index', function (require) {

    var Table = require('table');

    var HomeViewModel = function (data) {
        this.table = ko.observable(new Table(data));

        this.updateTableList = function (list) {
            this.table().setList(list);
        };
    };

    var init = function init() {
        var vm = new HomeViewModel([]);
        ko.applyBindings(vm, document.getElementById('home-body'));
        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            vm.updateTableList(data);
            $('select.custom-select').selectric('refresh');
        }).fail(function (jqXHR, textStatus, errorThrown) {
            debugger;
        });

        var closeNotification = document.querySelector('.new-notification-wp .new-notification .close-white-ico')
        if (closeNotification) {
            closeNotification.addEventListener('click', function () {
                this.parentElement.parentElement.remove();
            });
        }
    };

    return {
        init: init
    };
});