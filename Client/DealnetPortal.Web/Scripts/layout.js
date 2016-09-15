$(document)
    .ready(function () {
        $('.dealnet-sidebar-item a[href="' + window.location.pathname + '"]')
            .parents('.dealnet-sidebar-item')
            .addClass('dealnet-sidebar-item-selected');

        $('.dealnet-sidebar-item').click(function () {
            window.location.href = $(this).find('a').attr('href');
        });

        $('.dealnet-page-button').click(function () {
            window.location.href = $(this).find('a').attr('href');
        });

        var $body = $('body');
        $('.navbar-toggle').click(function(){
          var topOffset;
          if($('.navbar-collapse').attr('aria-expanded') === 'false'){
            topOffset = $(window).scrollTop();
            $body.css('top', -topOffset);
            $body.css('overflow', 'hidden');
            $('.overlay').show();
          }else{
            $body.css('top', -topOffset);
            $body.css('overflow', 'auto');
            $('.overlay').hide();
          }
        });

        $('.overlay').click(function(){
          $('.navbar-toggle').click();
          $body.css('overflow', 'auto');
          $(this).hide();
        })
    });

function showLoader() {
    $.loader({
        className: 'loader',
        content: '',
        width: 101,
        height: 100
});
}

function hideLoader() {
    $.loader('close');
}
