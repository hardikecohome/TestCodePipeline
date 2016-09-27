$(document)
    .ready(function () {
        $("input, textarea").placeholder();
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
        });

      addIconsToFields();
      toggleClearInputIcon();

      //$('select').each(function(){
      //  $('<option value="" selected>- not selected -</option>').prependTo($(this));
      //});

      $('select').on('change', function() {
        $(this).toggleClass("empty", $.inArray($(this).val(), ['', null]) >= 0);
      }).trigger('change');
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

function addIconsToFields(fields){
  var fields = fields || $('.control-group input, .control-group textarea');
  var fieldParent = fields.parent('.control-group:not(.date-group):not(.control-group-pass)');
  var fieldDateParent = fields.parent('.control-group.date-group');
  var fieldPassParent = fields.parent('.control-group.control-group-pass');
  var iconCalendar = $('<svg aria-hidden="true" class="icon icon-calendar"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-calendar"></use></svg>');
  var iconClearField = $('<a class="clear-input"><svg aria-hidden="true" class="icon icon-remove"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-remove"></use></svg></a>');
  var iconPassField = $('<svg aria-hidden="true" class="icon icon-eye"><use xlink:href="/client/Content/images/sprite/sprite.svg#icon-eye"></use></svg>');
  iconCalendar.appendTo(fieldDateParent);
  iconClearField.appendTo(fieldParent);
  iconPassField.appendTo(fieldPassParent);
  setTimeout(function(){
    fields.each(function(){
      if($(this).val().length > 0){
        $(this).siblings('.clear-input').show();
      }else{
        $(this).siblings('.clear-input').hide();
      }
    });
  }, 100);
}

function toggleClearInputIcon(fields){
  var fields = fields || $('.control-group input, .control-group textarea');
  var fieldParent = fields.parent('.control-group:not(.date-group):not(.control-group-pass)');
  fields.on('keyup', function(){
    if($(this).val().length !== 0){
      $(this).siblings('.clear-input').show();
    }else{
      $(this).siblings('.clear-input').hide();
    }
  });
  fieldParent.find('.clear-input').on('click', function(){
    $(this).siblings('input, textarea').val('');
    $(this).hide();
  });

}
