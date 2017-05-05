module.exports('my-profile-reducer', function (require) {
    var makeReducer = require('redux').makeReducer;
    var profileActions = require('my-profile-actions');
    var iniState = { };
    var setFormField = function (field) {
        return function (state, action) {
            var fieldObj = {};
            fieldObj[field] = action.payload;
            return $.extend({}, state, fieldObj);
        };
    };

    // your info reducer
    var reducerObj = {}
    reducerObj[profileActions.SET_INITIAL_STATE] = function (state, action) {
        return $.extend({}, state, action.payload);
    };
   
    var reducer = makeReducer(reducerObj, iniState);
    return reducer;
});