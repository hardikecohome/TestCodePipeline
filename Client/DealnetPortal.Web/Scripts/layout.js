$(document)
    .ready(function () {
      if(detectIE()){
        $('body').addClass('ie');
      }
       // fixMetaViewportIos();

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
        $('.navbar-toggle').trigger('click');
        $('body').removeClass('open-menu');
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
}

function resetScrollPosition(){
  var $body = $('body');
  var bodyOffset = Math.abs(parseInt($body.css('top')));
  $body.css('top', 'auto');
  $('html, body').scrollTop(bodyOffset);
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
  setTimeout(function(){
    $('select').each(function(){
      var selectClasses = $(this).hasClass("dealnet-disabled-input") ? "custom-select-disabled" : "custom-select";
      if(!$(this).parents(".ui-datepicker").length){
        if(!$(this).parents(".custom-select").length || !$(this).parents(".custom-select-disabled").length){
          $(this).wrap('<div class='+selectClasses+'>');
          if(detectIE() === false){
            $(this).after('<span class="caret">');
          }
        }
      }
    });
  }, 300);
}

function addIconsToFields(fields){
  var fields = fields || $('.control-group input:not(".dealnet-disabled-input"), .control-group textarea');
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

  /**
   * detect IE
   * returns version of IE or false, if browser is not Internet Explorer
   */
  function detectIE() {
    var ua = window.navigator.userAgent;

    // Test values; Uncomment to check result …

    // IE 10
    // ua = 'Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; Trident/6.0)';

    // IE 11
    // ua = 'Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko';

    // Edge 12 (Spartan)
    // ua = 'Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36 Edge/12.0';

    // Edge 13
    // ua = 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2486.0 Safari/537.36 Edge/13.10586';

    var msie = ua.indexOf('MSIE ');
    if (msie > 0) {
      // IE 10 or older => return version number
      return parseInt(ua.substring(msie + 5, ua.indexOf('.', msie)), 10);
    }

    var trident = ua.indexOf('Trident/');
    if (trident > 0) {
      // IE 11 => return version number
      var rv = ua.indexOf('rv:');
      return parseInt(ua.substring(rv + 3, ua.indexOf('.', rv)), 10);
    }

    var edge = ua.indexOf('Edge/');
    if (edge > 0) {
      // Edge (IE 12+) => return version number
      return parseInt(ua.substring(edge + 5, ua.indexOf('.', edge)), 10);
    }

    // other browser
    return false;
  }