module.exports('onboarding.index', function (require) {
    var company = require('onboarding.company');
    var product = require('onboarding.product');

    function init() {
        company.initCompany();
        $('#province-select').on('change', company.addProvince);
        product.initProducts();
        $('#offered-equipment').on('change', product.addProduct);
    }

    return {
        init: init
    }
})