module.exports('onboarding.success', function() {
    function initSendEmail() {
        $('#send-draft-email').on('submit',
            function(e) {
                var $this = $(this);
                e.preventDefault();
                if ($this.valid()) {
                    e.preventDefault();
                    $.ajax({
                        url: sendLinkUrl,
                        type: 'POST',
                        data: $this.serialize(),
                        success: function(data) {
                            if (data.success) {
                                $('#sent-success').removeClass('hidden');
                            } else {
                                $('#sent-success').addClass('hidden');
                            }
                        }
                    });
                }
            });
    }

    function initCopyLink() {
        var link = document.getElementById('resume-link');
        if (!$('body').is('.ios-device')) {

            var activeLink = '';

            $('#copy-resume-link').on('click',
                function() {
                    activeLink = link.value;
                    selectElement(link);
                    copyCommand();
                });

        } else {
            link = $('#resume-link');

            var linkVal = link.val();

            link.parent()
                .append($.parseHTML('<a href="' + linkVal + '" style="word-wrap: break-word;">' + linkVal + '</a>'));

            link.attr('type', 'hidden');
            $('#copy-resume-link').hide();
        }
    }

    function selectElement(el) {
        if (el.nodeName === "TEXTAREA" || el.nodeName === "INPUT")
            el.select();
        if (el.setSelectionRange && $('body').is('.ios-device'))
            el.setSelectionRange(0, 999999);
    }

    function copyCommand() {
        if (document.queryCommandSupported("copy")) {
            document.execCommand('copy');
        }
    }

    var init = function() {
        initSendEmail();
        initCopyLink();
    }

    return init;
})