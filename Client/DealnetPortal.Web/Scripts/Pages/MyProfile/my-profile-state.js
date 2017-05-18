module.exports('my-profile-state', function () {
    var state = {
        postalCodes: [],
        categories: [],
        postalCodeSecondId: 0,
        categorySecondId: 0
    };

    window.state = state;

    return state;
})