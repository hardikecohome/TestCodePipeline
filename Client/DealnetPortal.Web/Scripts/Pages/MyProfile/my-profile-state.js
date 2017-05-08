module.exports('my-profile-state', function () {
    var state = {
        postalCodes: [],
        postalCodeSecondId: 0
    };

    window.state = state;

    return state;
})