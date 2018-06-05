﻿module.exports('paginator', function () {

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
            var activePage = this.pageIndex();
            var maxPage = this.maxPageIndex();
            var maxButtons = 5;

            var startPage = 0;
            var endPage = 0;
            var count = this.list().length || 0;
            if (maxButtons < count) {
                startPage = Math.max(Math.min(activePage - Math.floor(maxButtons / 2, 10), count - maxButtons + 1), 1);
                var tempEnd = startPage + maxButtons - 1;
                endPage = maxPage < tempEnd ? maxPage : tempEnd;
            } else {
                startPage = 1;
                endPage = maxPage;
            }

            for (var i = startPage; i <= endPage; i++) {
                pages.push({
                    pageNumber: i - 1,
                    active: activePage == i - 1,
                    disabled: false,
                    pageText: i
                });
            }

            if (startPage > 1 && startPage > 2) {
                var first = pages.shift();
                pages.unshift({
                    pageText: '...',
                    active: false,
                    disabled: true
                });
                pages.unshift({
                    pageNumber: 0,
                    active: activePage == 0,
                    disabled: false,
                    pageText: '1'
                });
            }

            if (endPage < maxPage && endPage < maxPage - 1) {
                var last = pages.pop();
                pages.push({
                    pageText: '...',
                    active: false,
                    disabled: true
                });
                pages.push({
                    pageNumber: maxPage,
                    active: activePage == maxPage - 1,
                    disabled: false,
                    pageText: maxPage
                });
            }

            return pages;
        }, this);

        this.goToFirstPage = function () {
            this.moveToPage(0);
        };

        this.goToLastPage = function () {
            this.moveToPage(this.maxPageIndex());
        };

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
    };

    return Paginator;
});