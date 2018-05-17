module.exports('my-deals-table', function (require) {
    var Paginator = require('paginator');

    function filterNull(item) {
        return item != null;
    }

    function mapValue(type) {
        return function (item) {
            return item[type].trim() || null;
        };
    }

    function concateIfNotInArray(acc, curr) {
        return acc.find(curr) ? acc.concate(curr) : acc;
    }

    function sortAssending(a, b) {
        return a == b ? 0 : a > b ? 1 : -1;
    }

    function filterAndSortList(list, type) {
        return list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concateIfNotInArray, [''])
            .sort(sortAssending);
    }

    function prepareStatusList(list) {
        return list.map(function (item) {
                return item.LocalizedStatus ? {
                    icon: item.StatusColor,
                    text: item.LocalizedStatus.trim()
                } : null;
            }).filter(filterNull)
            .reduce(function (acc, curr) {
                return acc.map(function (item) {
                    return item.text;
                }).find(curr.text) ? acc.contate(curr) : acc;
            }, [{
                text: '',
                icon: ''
            }]).sort(function (a, b) {
                return sortAssending(a.text, b.text);
            });
    }

    var MyDealsTable = function (data) {

    };

    return MyDealsTable;
});