﻿module.exports('home-page-index', function (require) {

    var Table = require('table');

    var HomeViewModel = function (data) {
        var table = new Table(data);

        this.updateTableList = function (list) {
            this.table.setList(list);
        }
    }

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
        })
    }

    return {
        init: init
    };
});