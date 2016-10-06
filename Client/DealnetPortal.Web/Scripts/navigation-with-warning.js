﻿function navigateWithWarning(event) {
    console.log(event);
    var url = event.target.href;
    var message = "If you change Home Owner Information you will have to pass Credit Check step again"
    $('#alertModal').find('.modal-body p').html(message);
    $('#alertModal').find('.modal-title').html('Navigate to step 1?');
    $('#alertModal').find('#confirmAlert').html('Go to Step 1');
    $('#alertModal').modal('show');
    $('#confirmAlert').on('click', function(){
        window.location.href = url;
    });
    event.preventDefault();
    /*if (!confirm('Are you sure you want to navigate to previous steps? You will have to pass Credit Check step again.')) {
        event.preventDefault();
    }*/
}