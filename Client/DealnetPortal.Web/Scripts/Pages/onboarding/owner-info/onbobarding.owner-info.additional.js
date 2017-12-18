﻿module.exports('onboarding.owner-info.additional', function (require) {
    var state = require('onboarding.state').state;
    var constants = require('onboarding.state').constants;
    var manageOwners = require('onboarding.owner-info.conversion');
    var resetForm = require('onboarding.common').resetFormValidation;
    var recalculateTotalPercentage = require('onboarding.owner-info.setters').recalculateTotalPercentage;

    var clearAddress = require('clearAddress');

    var addAditionalOwner = function (ownerIndex) {
        if ($('#additional-owner-warning').is(':visible')) {
            $('#additional-owner-warning').addClass('hidden');
        }

        var newOwnerState = {};
        newOwnerState[ownerIndex] = { requiredFields: constants.requiredFields.slice() };

        $.extend(state['owner-info']['owners'], newOwnerState);

        var template = document.getElementById('template').innerHTML;
        var nextOwnerIndex = state['owner-info']['nextOwnerIndex'];

        var result = template.split('Owners[0]')
            .join('Owners[' + nextOwnerIndex + ']')
            .split('Owners_0').join('Owners_' + nextOwnerIndex);

        if (constants.maxAdditionalOwner === state['owner-info']['nextOwnerIndex']) {
            $('#add-additional').addClass('hidden');
        }

        var $result = $($.parseHTML(result));
        $('#additional-owners').append($result);

        $result.find('input[id^="owner0"]').each(function () {
            var $this = $(this);
            var data = Object.assign({}, $('#' + this.id).data());
            $this.attr('id', $this.attr('id').replace('owner0', 'owner' + nextOwnerIndex));
            Object.keys(data).forEach(function (key) {
                $this.attr('data-' + key.toDash(), data[key]);
            });
        });

        $result.find('select[id^="owner0"]').each(function () {
            var $this = $(this);
            var data = Object.assign({}, $('#' + this.id).data());
            $this.attr('id', $this.attr('id').replace('owner0', 'owner' + nextOwnerIndex));
            Object.keys(data).forEach(function (key) {
                $this.attr('data-' + key.toDash(), data[key]);
            });
        });

        $result.find('#owner-remove').attr('id', 'owner' + nextOwnerIndex + '-remove');

        $result.find('svg').each(function (index, node) {
            var clone = node.cloneNode(true);
            node.parentNode.replaceChild(clone, node);
        });

        $('#owner-container').attr('id', 'owner' + nextOwnerIndex + '-container');

        toggleClearInputIcon($('#owner' + nextOwnerIndex + '-container').find('input'));

        $('#owner' + nextOwnerIndex + '-container')
            .find('.clear-input')
            .find('svg')
            .html('<use xlink:href="' + urlContent + 'Content/images/sprite/sprite.svg#icon-remove"></use>');

        $('#owner' + nextOwnerIndex + '-container')
            .find('.clear-address').on('click', clearAddress);

        $('#owner' + nextOwnerIndex + '-container').find('span.text-danger').each(function () {
            if ($(this).has('span')) {
                $(this).children().text('');
            }
        });

        state['owner-info']['nextOwnerIndex']++;
        resetForm('#onboard-form');
        recalculateTotalPercentage();
    }

    var removeAdditionalOwner = function (ownerNumber) {
        $('#' + ownerNumber + '-container').off();
        $('#' + ownerNumber + '-container').remove();
        manageOwners.recalculateOwnerIndex(ownerNumber);
        state['owner-info'].nextOwnerIndex--;
    }

    return {
        add: addAditionalOwner,
        remove: removeAdditionalOwner
    }
})