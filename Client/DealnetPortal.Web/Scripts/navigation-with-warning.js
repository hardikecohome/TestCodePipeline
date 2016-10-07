function navigateWithWarning(event) {
    //console.log(event); /*This part of code allow user go to href url and the code below doesn't work in IE browsers*/

    var url = event.target.href;
    var stepName = event.toElement.innerText;
    /*console.log(event.target.text());*/
    var message = "If you change Home Owner Information you will have to pass Credit Check step again"
    $('#alertModal').find('.modal-body p').html(message);
    $('#alertModal').find('.modal-title').html('Navigate to step '+stepName+'?');
    $('#alertModal').find('#confirmAlert').html('Go to Step '+stepName);
    $('#alertModal').modal('show');
    $('#confirmAlert').on('click', function(){
        window.location.href = url;
    });

    event.preventDefault();
    /*if (!confirm('Are you sure you want to navigate to previous steps? You will have to pass Credit Check step again.')) {
        event.preventDefault();
    }*/
}