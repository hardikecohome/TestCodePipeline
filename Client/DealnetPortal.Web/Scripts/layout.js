$(document)
    .ready(function () {

        var selected = getValueFromLocalStorage('dealnet-sidebar-selected-item', 'sidebar-item-homepage');
        var selectedItem = $('#' + selected).addClass('dealnet-sidebar-item-selected');

        var link = selectedItem.find('a').attr('href').toLowerCase();
        var location = window.location.pathname.toLowerCase();
        if (link && location != link.toLowerCase() && !location.indexOf('/account',0)) {
            window.location.href = link;
        }
        var sidebarItems = $('.dealnet-sidebar-item');
        sidebarItems.click(function () {
            sidebarItems.removeClass('dealnet-sidebar-item-selected');
            $(this).addClass('dealnet-sidebar-item-selected');
            localStorage.setItem('dealnet-sidebar-selected-item', $(this).attr('id'));
            window.location.href = $(this).find('a').attr('href');
        });

    });


function getValueFromLocalStorage(key, defaultValue) {
    var item = localStorage.getItem(key);
    if (item) {
        return item;
    } else {
        console.log('Item with key ' + key + ' not found in local storage. Default value "' + defaultValue + '" used');
        return defaultValue;
    }
};