function navigateWithWarning(event) {
    console.log(event);
    var url = event.target.href;
    var message = "Are you sure you want to navigate to previous steps? You will have to pass Credit Check step again."
    $('#alertModal').find('.modal-body p').html(message);
    $('#alertModal').find('.modal-title').html('<svg aria-hidden="true" class="icon icon-reject"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-reject"></use></svg> Warning');
    $('#alertModal').modal('show');
    $('#confirmAlert').on('click', function(){
        window.location.href = url;
    });
    event.preventDefault();
    /*if (!confirm('Are you sure you want to navigate to previous steps? You will have to pass Credit Check step again.')) {
        event.preventDefault();
    }*/
}