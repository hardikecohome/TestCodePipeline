module.exports('my-profile-template', function () {
    var equipmentFactory = function (parentNode, options) {
        var template = document.getElementById('postal-template').innerHTML;
        var result = template.split("PostalCodes[0]")
            .join("PostalCodes[" + options.id + "]")
            .split("PostalCodes_0").join("PostalCodes_" + options.id);

        parentNode.append($.parseHTML(result));

        $(parentNode).attr('id', 'postal-code-' + options.id);
        $(parentNode).find('#remove-postal-code-')
            .attr('hidden-value', options.id)
            .attr('id', 'remove-postal-code-' + options.id);
        $(parentNode).find('#PostalCodes_' + options.id + '__Value', '').val('');

        return parentNode;
    };
    return equipmentFactory;
});