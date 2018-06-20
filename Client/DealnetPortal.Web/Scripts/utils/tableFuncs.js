module.exports('tableFuncs', function () {

    function filterNull(item) {
        return item !== undefined && item !== null;
    }

    function mapValue(type) {
        return function (item) {
            return item[type] || null;
        };
    }

    function concatIfNotInArray(acc, curr) {
        return acc.indexOf(curr) === -1 ? acc.concat(curr) : acc;
    }

    function sortAscending(a, b) {
        if (typeof a == 'string' && typeof b == 'string') {
            return a.toLocaleLowerCase().localeCompare(b.toLocaleLowerCase());
        }
        return a == b ? 0 : a > b ? 1 : -1;
    }

    function sortDescending(a, b) {
        if (typeof a == 'string' && typeof b == 'string') {
            return b.toLocaleLowerCase().localeCompare(a.toLocaleLowerCase());
        }
        return a == b ? 0 : a < b ? 1 : -1;
    }

    function filterAndSortList(list, type) {
        return list.map(mapValue(type))
            .filter(filterNull)
            .reduce(concatIfNotInArray, [''])
            .sort(sortAscending);
    }

    function prepareStatusList(list) {
        return list.map(function (item) {
                return item.LocalizedStatus ? {
                    icon: item.StatusColor,
                    text: item.LocalizedStatus
                } : null;
            }).filter(filterNull)
            .reduce(function (acc, curr) {
                return acc.map(function (item) {
                        return item.text;
                    }).indexOf(curr.text) === -1 ?
                    acc.concat(curr) :
                    acc;
            }, [{
                text: '',
                icon: 'grey'
            }])
            .sort(function (a, b) {
                return sortAscending(a.text, b.text);
            });
    }

    return {
        filterNull: filterNull,
        mapValue: mapValue,
        concatIfNotInArray: concatIfNotInArray,
        sortAscending: sortAscending,
        sortDescending: sortDescending,
        filterAndSortList: filterAndSortList,
        prepareStatusList: prepareStatusList
    };
});