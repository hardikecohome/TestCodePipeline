module.exports('postalCode-template', function () {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById('postal-template').innerHTML;
        var result = template.split("PostalCodes[0]")
            .join("PostalCodes[" + options.id + "]")
            .split("PostalCodes_0").join("PostalCodes_" + options.id);

        parentNode.append(result);

        $(parentNode).find('#PostalCodes_' + options.id + '__Id').val(0);
        $(parentNode).attr('id', 'postal-code-' + options.id);
        $(parentNode).find('#remove-postal-code-')
            .attr('hidden-value', options.id)
            .attr('id', 'remove-postal-code-' + options.id);
        $(parentNode).find('#PostalCodes_' + options.id + '__PostalCode', '').val('');

        return parentNode;
    };
    return equipmentFactory;
});