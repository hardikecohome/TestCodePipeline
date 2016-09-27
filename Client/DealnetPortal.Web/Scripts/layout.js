$(document)
    .ready(function () {
        //fixMetaViewportIos();

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

      $(document).on('show.bs.modal', function () {
        saveScrollPosition();
        console.log('open');
      }).on('hidden.bs.modal', function () {
        if($('.modal:visible').length == 0) {
          resetScrollPosition();
        }
      });

      $('.navbar-toggle').click(function(){
        if($('.navbar-collapse').attr('aria-expanded') === 'false'){
          saveScrollPosition();
          $('body').addClass('open-menu');
          $('.overlay').show();
        }else{
          $('body').removeClass('open-menu');
          resetScrollPosition();
          $('.overlay').hide();
        }
      });
      $('.overlay').click(function(){
        resetScrollPosition();
        $(this).hide();
      });

      addIconsToFields();
      toggleClearInputIcon();
      customizeSelect();

      if($('.summary-info-hold #basic-info-form .credit-check-info-hold .col-md-6').length % 2 !== 0){
        $('.summary-info-hold #contact-info-form.credit-check-info-hold').addClass('shift-to-basic-info');
      }
    });

function showLoader() {
    $.loader({
        className: 'loader',
        content: '',
        width: 101,
        height: 100
  });
}

function saveScrollPosition(){
  var $body = $('body');
  //if open one modal right after other one
  var topOffset = $(window).scrollTop();
  $body.css('top', -topOffset);
  console.log('asd');
}

function resetScrollPosition(){
  var $body = $('body');
  var bodyOffset = Math.abs(parseInt($body.css('top')));
  $body.css('top', 'auto');
  $('html, body').scrollTop(bodyOffset);
  console.log('bbbb');
}

function hideLoader() {
    $.loader('close');
}

function fixMetaViewportIos(){
  if(/iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream){
    document.querySelector('meta[name=viewport]')
      .setAttribute(
        'content',
        'initial-scale=1.0001, minimum-scale=1.0001, maximum-scale=1.0001, user-scalable=no'
      );
  }
}

function customizeSelect(){
  $('select').each(function(){
    $(this).wrap('<div class="custom-select">').after('<span class="caret">');
    $('<option value="">- not selected -</option>').prependTo($(this));
  });
  console.log($('select').val())
  $('select').on('change', function() {
    $(this).toggleClass("empty", $.inArray($(this).val(), ['', null]) >= 0);
  }).trigger('change');
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
