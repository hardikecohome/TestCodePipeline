module.exports('my-profile-index', function(require) {
    var observe = require('redux').observe;
    var createAction = require('redux').createAction;

    var initMyProfileView = require('my-profile-view');

    var profileStore = require('my-profile-store');
    var dispatch = profileStore.dispatch;

    // view layer
    initMyProfileView(profileStore);

    var observeProfileFormStore = observe(profileStore);

    observeProfileFormStore(function (state) { return { };})(function (props) { });
});