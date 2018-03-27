module.exports('paginator', function () {

    var Paginator = function (list, size) {
        this.pageSizes = ko.observableArray([10, 25, 50, 100]);
        this.pageSize = ko.observable(size || 10);
        this.list = ko.observableArray(list);

        this.pageIndex = ko.observable(0);

        this.pagedList = ko.computed(function () {
            var start = this.pageIndex() * this.pageSize();
            return this.list().slice(start, start + this.pageSize());
        }, this);

        this.maxPageIndex = ko.computed(function () {
            var cil = Math.ceil(this.list().length / this.pageSize());
            return cil > 0 ? cil - 1 : cil;
        }, this);

        this.allPages = ko.computed(function () {
            var pages = [];
            var maxPageIndex = this.maxPageIndex()
            for (var i = 0; i <= maxPageIndex; i++) {
                pages.push(i);
            }
            return pages;
        }, this);

        this.goToFirstPage = function () {
            this.moveToPage(0);
        }

        this.goToLastPage = function () {
            this.moveToPage(this.maxPageIndex());
        }

        this.goToNextPage = function () {
            this.moveToPage(this.pageIndex() + 1);
        }

        this.goToPreviousPage = function () {
            this.moveToPage(this.pageIndex() - 1);
        }

        this.moveToPage = (function (index) {
            this.pageIndex(index);
        }).bind(this);
    };

    return Paginator;
});