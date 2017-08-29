module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');
    var ownerInfo = require('onboarding.owner-info.index');

    function init() {
        company.initCompany();
        $('#province-select').on('change', company.addProvince);
        product.initProducts();
        $('#offered-equipment').on('change', product.addProduct);
        $('.add-new-brand-link').on('click', product.addBrand);
        ownerInfo.init();
        $('#province-select').on('change', addProvince);
    }

    return {
        init: init
    }
})