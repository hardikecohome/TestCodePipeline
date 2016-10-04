function navigateWithWarning(event) {
    console.log(event);
    if (!confirm('Are you sure you want to navigate to previous steps? You will have to pass Credit Check step again.')) {
        event.preventDefault();
    }
}