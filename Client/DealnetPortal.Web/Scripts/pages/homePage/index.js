module.exports('home-page-index', function (require) {

    var Table = require('table');

    var HomeViewModel = function (data) {
        this.table = ko.observable(new Table(data));
    }

    var init = function init() {
        $.ajax(itemsUrl, {
            cache: false,
            mode: 'GET'
        }).done(function (data) {
            ko.applyBindings(new HomeViewModel(data), document.getElementById('home-body'));
            $('select.custom-select').selectric('refresh');
        }).fail(function (jqXHR, textStatus, errorThrown) {
            debugger;
        })
    }

    return {
        init: init
    };
});