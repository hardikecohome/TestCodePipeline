﻿module.exports('onboarding.state', function () {
    var state = {
        selectedProvinces: [],
        selectedEquipment: [],
        nextProvinceId: 0,
        nextEquipmentId: 0,
        nextBrandNumber:2
    };

    var constants = {

    };

    window.state = state;

    return {
        state: state,
        constants: constants
    };
});
