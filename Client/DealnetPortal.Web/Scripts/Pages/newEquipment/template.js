module.exports('equipment-template', function() {
    var equipmentFactory = function(parentNode, options) {
        var template = '' +
            '<div class="dealnet-middle-header equipment-number-header">' +
                '<span class="equipment-number"> Nº' + options.index + '</span>' +
                '<div class="additional-remove">' +
                    '<i class="glyphicon glyphicon-remove"></i>' +
                '</div>' +
            '</div>' +
            '<div class="row">' +
                '<div class="col-md-3">' +
                    '<div class="form-group">' +
                        '<label for="NewEquipment_0__Type">Type</label>'+
                        '<select class="form-control">'+
                            '<option selected="selected">-not selected-</option>'+
                            '<option>Air Conditioner</option>'+
                            '<option>Air Handler</option>'+
                        '</select>'+
                    '</div>'+
                '</div>'+
                '<div class="col-xs-12 col-md-6">'+
                    '<div class="form-group">'+
                        '<label>Description</label>'+
                        '<div class="control-group">'+
                            '<input class="form-control dealnet-input" placeholder="Description" type="text">'+
                        '</div>'+
                    '</div>'+
                '</div>'+
                '<div class="col-md-3">'+
                    '<div class="form-group">'+
                        '<label>Cost</label>'+
                        '<div class="control-group has-addon-left">'+
                            '<div class="control-group-addon">$</div>'+
                            '<input class="form-control dealnet-input equipment-cost" placeholder="Cost" type="text">'+
                        '</div>'+
                    '</div>'+
                '</div>'+
            '</div>';

        parentNode.append($.parseHTML(template));

        return parentNode;
    };

    return equipmentFactory;
});