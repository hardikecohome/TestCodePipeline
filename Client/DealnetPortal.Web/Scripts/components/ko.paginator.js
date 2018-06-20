module.exports('paginator', function () {

    var Paginator = function (list, size) {
        this.pageSizes = ko.observableArray([10, 25, 50, 100]);
        this.pageSize = ko.observable(size || 10);
        this.list = ko.observableArray(list);

        this.pageIndex = ko.observable(0);

        this.pagedList = ko.computed(function () {
            var size = Number(this.pageSize());
            var start = this.pageIndex() * size;
            return this.list().slice(start, start + size);
        }, this);

        this.maxPageIndex = ko.computed(function () {
            var size = Number(this.pageSize());
            var cil = Math.ceil(this.list().length / size);
            return cil > 0 ? cil - 1 : cil;
        }, this);

        this.allPages = ko.computed(function () {
            var pages = [];
            var activePage = this.pageIndex();
            var maxPage = this.maxPageIndex();
            var maxButtons = 7;

            var startPageIndex = 0;
            var endPageIndex = maxPage;
            if (maxButtons < maxPage) {
                startPageIndex = Math.max(Math.min(activePage - Math.floor(maxButtons / 2, 10), maxPage - maxButtons + 1), 0);
                var tempEnd = startPageIndex + maxButtons - 1;
                endPageIndex = maxPage < tempEnd ? maxPage : tempEnd;
            }

            for (var i = startPageIndex; i <= endPageIndex; i++) {
                pages.push({
                    pageNumber: i,
                    active: activePage == i,
                    disabled: activePage == i,
                    pageText: i + 1
                });
            }

            if (pages.length >= maxButtons && activePage > 2) {
                var first = pages.shift();
                var second = pages.shift();

                pages.unshift({
                    pageText: '...',
                    active: false,
                    disabled: true
                });
                pages.unshift({
                    pageNumber: 0,
                    active: activePage == 0,
                    disabled: activePage == 0,
                    pageText: '1'
                });
            }

            if (pages.length >= maxButtons && activePage < maxPage - 2) {
                var last = pages.pop();
                var end = pages.pop();

                pages.push({
                    pageText: '...',
                    active: false,
                    disabled: true
                });
                pages.push({
                    pageNumber: maxPage,
                    active: activePage == maxPage,
                    disabled: activePage == maxPage,
                    pageText: maxPage + 1
                });
            }

            return pages;
        }, this);

        this.goToNextPage = function () {
            var pageIndex = this.pageIndex();
            this.moveToPage({
                pageNumber: pageIndex + 1,
                disabled: pageIndex == this.maxPageIndex()
            });
        };

        this.goToPreviousPage = function () {
            var pageIndex = this.pageIndex();
            this.moveToPage({
                pageNumber: pageIndex - 1,
                disabled: pageIndex == 0
            });
        };

        this.moveToPage = (function (page) {
            if (!page.disabled)
                this.pageIndex(page.pageNumber);
        }).bind(this);

        this.pageSize.subscribe(function () {
            this.pageIndex(0);
        }, this);

        this.list.subscribe(function () {
            this.pageIndex(0);
        }, this);
    };

    return Paginator;
});