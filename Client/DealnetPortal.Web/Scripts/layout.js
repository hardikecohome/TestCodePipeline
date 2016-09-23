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
            $body.addClass('open-menu');
            $('.overlay').show();
          }else{
            $body.css('top', -topOffset);
            $body.css('overflow', 'auto');
            $body.removeClass('open-menu');
            $('.overlay').hide();
          }
        });

        $('.overlay').click(function(){
          $('.navbar-toggle').click();
          $body.css('overflow', 'auto');
          $body.removeClass('open-menu');
          $(this).hide();
        })

      $('.control-group .clear-input').each(function(){
        if($(this).siblings('input').val() === ""){
          $(this).hide();
        }else{
          $(this).show();
        }
      });
      $('.control-group input').on('keyup', function(){
        if($(this).val().length !== 0){
          $(this).siblings('.clear-input').show();
        }else{
          $(this).siblings('.clear-input').hide();
        }
      });
      $('.control-group .clear-input').on('click', function(){
        $(this).siblings('input').val('');
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
