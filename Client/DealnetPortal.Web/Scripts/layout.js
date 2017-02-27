$(document)
    .ready(function () {
      var isMobile = {
        Android: function() {
          return navigator.userAgent.match(/Android/i);
        },
        BlackBerry: function() {
          return navigator.userAgent.match(/BlackBerry/i);
        },
        iOS: function() {
          return navigator.userAgent.match(/iPhone|iPad|iPod/i);
        },
        Opera: function() {
          return navigator.userAgent.match(/Opera Mini/i);
        },
        Windows: function() {
          return navigator.userAgent.match(/IEMobile/i);
        },
        any: function() {
          return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
        }
      };

      setDeviceClasses();


      if(detectIE()){
        $('body').addClass('ie');
      }
      $('.chosen-language-link').on('click', function(){
        $(this).parents('.lang-switcher').toggleClass('open');
        return false;
      });

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
      }).on('shown.bs.modal', function(){
        $('textarea').each(function(){
          has_scrollbar($(this), 'textarea-has-scroll');
        });
      }).on('hidden.bs.modal', function () {
        if(isMobile.iOS() && viewport().width >= 768){
          resetModalDialogMarginForIpad();
        }

        if($('.modal:visible').length == 0) {
          resetScrollPosition();
        }
      });

      $('#alertModal').on('hidden.bs.modal', function () {
          $('#confirmAlert').off('click');
      });

      $('.navbar-toggle').click(function(){
        if($('.navbar-collapse').attr('aria-expanded') === 'false'){
          saveScrollPosition();
          $('body').addClass('open-menu');
          $('body').addClass('menu-animated');
          $('.overlay').show();
        }else{
          $('body').removeClass('open-menu');
          resetScrollPosition();
          $('.overlay').hide();
          setTimeout( function(){
            $('body').removeClass('menu-animated');
          }, 400);
        }
      });

      $('.overlay').click(function(){
        $('.navbar-toggle').trigger('click');
        $('body').removeClass('open-menu');
        resetScrollPosition();
        $(this).hide();
        setTimeout( function(){
          $('body').removeClass('menu-animated');
        }, 400);
      });

      $('.credit-check-info-hold.fit-to-next-grid').each(function(){

        if($(this).find('.grid-column').length % 2 !== 0){
          $(this).parents('.grid-parent').next('.credit-check-info-hold').addClass('shift-to-basic-info');
          $(this).parents('.grid-parent').next('.grid-parent').find('.credit-check-info-hold').addClass('shift-to-basic-info');
        }

      });

      $('.dealnet-disabled-input, input.control-disabled, textarea.control-disabled, select.control-disabled').each(function(){
        $(this).attr('type', 'hidden');
        var inpValue = $(this).is('select')? $(this).find("option:selected").text() : $(this).is('textarea')? $(this).text() : $(this).val();
        if($(this).is('.control-disabled')){
          $(this).after($('<div/>',{
            class: "control-disabled",
            text: inpValue
          }));
        }else{
          $(this).after($('<div/>',{
            class: "dealnet-disabled-input dealnet-disabled-input-value",
            html: inpValue.replace(/\r?\n/g, '<br />')
          }));
        }
      });


      $(window).on('scroll', function(){
        detectPageHeight();
      }).on('resize', function(){
        setDeviceClasses();
        if(isMobile.iOS() && viewport().width >= 768){
          if($('.modal.in').length === 1){
            setModalMarginForIpad();
          }
        }
        detectPageHeight();
        documentsColHeight();
      });

      $('.reports-contract-item').each(function(){
        $('.contract-hidden-info').hide();
      });
      $('.show-full-conract-link').on('click', function(){
        $(this).parents('.reports-contract-item').find('.contract-hidden-info').show();
        $(this).hide();
        return false;
      });

      $('#steps .step-item[data-warning="true"]').on('click', function(){
        if($(this).attr('href')){
          navigateToStep($(this));
        }
        return false;
      });

      setTimeout(function(){
        documentsColHeight();
        $('.credit-check-info-hold .dealnet-credit-check-section').each(function(){
          var col = $(this).parents('.col-md-6');
          if(col.not('.col-md-push-6')){
            var colOffset = col.position().left;
            if(colOffset == 0 && col.next('.col-md-6').length){
              col.addClass('has-right-border');
            }
          }
          if(col.is('.col-md-push-6')){
            var colOffset = col.next('.col-md-pull-6').position().left;
            if(colOffset == 0 && col.next('.col-md-pull-6').length){
              col.next('.col-md-pull-6').addClass('has-right-border');
            }
          }
        });
      }, 500);

      $('.scanlicence-info-link').on('click', function(){
        $(this).toggleClass('active');
        return false;
      });


      if($('.loan-sticker').length && viewport().width >= 768){
        $('.loan-sticker').each(function(){
          if(isMobile.iOS()){
            stickySection($(this), 'tablet-ios');
          }else{
            stickySection($(this), 'all');
          }
        });
      }


      addIconsToFields();
      toggleClearInputIcon();
      customizeSelect();
      commonDataTablesSettings();
      recoverPassword();


        /*Settings for propper work of datepicker inside bootstrap modal*/

        $.fn.modal.Constructor.prototype.enforceFocus = function () {};

        /*END Settings for propper work of datepicker inside bootstrap modal*/

      var resizeInt = null;
      $('textarea').each(function(){
        var textField = $(this);
        setTimeout(function(){
          has_scrollbar(textField, 'textarea-has-scroll');
        }, 100);

        textField.on("mousedown", function(e) {
          resizeInt = setInterval(function(){
           has_scrollbar(textField, 'textarea-has-scroll');
         }, 1000/15);
        });
      });

      $('textarea').on('keyup', function(){
        has_scrollbar($(this), 'textarea-has-scroll');
      });
      $(window).on("mouseup", function(e) {
        if (resizeInt !== null) {
          clearInterval(resizeInt);
        }
        //resizeEvent();
      });

      /*Responsive tabs*/
      if($('.responsive-tabs').length){
        var uploadErrorIcon = '<span class="error-icon"><svg aria-hidden="true" class="icon icon-error-upload"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-error-upload"></use></svg></span>';
        var uploadSuccess = '<span class="custom-checkbox"><span class="checkbox-icon"><svg aria-hidden="true" class="icon icon-checked"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-checked"></use></svg></span></span>';
        var accordionArrows = '<span class="pill-arrow"><i class="glyphicon"></i></span>';

        var uploadedStatus = $('<span>', {
          class: 'upload-doc-status',
          html: uploadErrorIcon + uploadSuccess});
        uploadedStatus.prependTo('a[data-toggle="tab"]');
        $(accordionArrows).appendTo('a[data-toggle="tab"]');

        $('.responsive-tabs a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
          var currentTab = $(this);
          var currentTabParent = currentTab.parents('.documents-pills-item');
          var currentTabId = currentTab.attr('aria-controls');

          $('a[data-toggle="tab"]').parents('.documents-pills-item').removeClass('active');
          $('a[aria-controls="'+ currentTabId +'"]').parents('.documents-pills-item').addClass('active');
          currentTab.parents('.documents-pills-item').addClass('active');
        });
      }
});

function scrollPageTo(elem){
  if(elem.offset().top < $(window).scrollTop() || elem.offset().top > $(window).scrollTop()+ window.innerHeight){
    $('html, body').animate({
      scrollTop: elem.offset().top - elem.outerHeight(true) - 10
    }, 2000);
  }
}

function resetModalDialogMarginForIpad(){
  $('.modal.in').find('.modal-dialog').css({
    'margin-bottom':  30 + 'px'
  });
}

function setModalMarginForIpad(){
  if(window.innerHeight < window.innerWidth){
    keyboardHeight = 60
  }else{
    keyboardHeight = 40
  }
  $('.modal.in').find('.modal-dialog').css({
    'margin-bottom':  keyboardHeight + 'vh'
  });
}

function fixedOnKeyboardShownIos(fixedElem){
  var $fixedElement = fixedElem;
  var topPadding = 10;

  function fixFixedPosition() {
    var absoluteTopCoord =  ($(window).scrollTop() - fixedElem.parent().offset().top ) + topPadding;

    $fixedElement.addClass('absoluted-div').css({
      top: absoluteTopCoord + 'px',
    }).fadeIn('fast')
  }
  function resetFixedPosition() {
    $fixedElement.removeClass('absoluted-div').css({
      top: $('.navbar-header').height() + topPadding,
    });
    $(document).off('scroll', updateScrollTop);
    resetModalDialogMarginForIpad();
  }
  function updateScrollTop() {
    var absoluteTopCoord =  ($(window).scrollTop() - fixedElem.parent().offset().top ) + topPadding;
    $fixedElement.css('top', absoluteTopCoord + 'px');
  }

  $('input, textarea, [contenteditable=true], select').on({
    focus: function() {
      if($(this).parents('.modal.in').length === 1){
        setModalMarginForIpad();
      }else{
        setTimeout(fixFixedPosition, 100);
      }
      $(document).scroll(updateScrollTop);
    },
    blur: resetFixedPosition
  });
}

function updateModalHeightIpad(){

  //If focus on input inside modal in new added blocks (which were display none when modal appears)

  if($('body').is('.ios-device.tablet-device') && viewport().width >= 768) {
    $('input, textarea, [contenteditable=true], select').on({
      focus: function() {
        setModalMarginForIpad();
      },
      blur: function(){
        resetModalDialogMarginForIpad();
      }
    });
  }
}

function stickySection(elem, device){
  var fixedHeaderHeight,
      parentDiv = elem.parents('.sticker-parent'),
      windowTopPos,
      parentOffsetTop,
      parentOffsetLeft,
      stickerTopPos,
      stickerWidth,
      stickerLeftPos,
      topPadding = 10,
      device = device || 'all'; // iOS or other devices(desktop and android)

    $(window).on('scroll resize', function(){
      fixedHeaderHeight = $('.navbar-header').height();
      windowTopPos = $(window).scrollTop() + fixedHeaderHeight;
      parentOffsetTop = parentDiv.offset().top;
      parentOffsetLeft = parentDiv.offset().left;
      stickerWidth = parentDiv.width();
      stickerLeftPos = parentOffsetLeft;


        if (windowTopPos >= parentOffsetTop) {
          elem.addClass("stick");
          stickerTopPos = fixedHeaderHeight + topPadding;

          if(device === 'tablet-ios'){
            fixedOnKeyboardShownIos(elem);
          }else{
            elem.css({
              top: stickerTopPos + 'px',
              width: 'auto',
              right: 0,
              left: stickerLeftPos + 'px'
            });
          }
        } else {
          elem.removeClass("stick");
          stickerTopPos = 0;
        }
    });
}


function has_scrollbar(elem, className)
{
  elem_id = elem.attr('id');
  if (elem[0].clientHeight < elem[0].scrollHeight)
    elem.parents('.control-group').addClass(className);
  else
    elem.parents('.control-group').removeClass(className);
}

function inputDateFocus(input){
  input.on('click touchend', function(){
    if(viewport().width < 768)
    {
      if($('body').not('.hasDatepicker')){
        $('body').addClass('hasDatepicker');
      }
    }
  }).on('focus', function(){
    setTimeout(customDPSelect, 0);
    if(!navigator.userAgent.match(/(iPod|iPhone|iPad)/)){
      $(this).blur()
        .addClass('focus');
    }
  });
}

function onDateSelect(input){
  input
    .removeClass('focus');
  $('body').removeClass('hasDatepicker');
}

function documentsColHeight(){
  var columns = $('.report-documents-list .document-col');
  /*console.log(columns.find('.documents-inner').height());*/
  columns.find('.dealnet-credit-check-section').css('min-height', columns.find('.documents-inner').height());
}

function navigateToStep(targetLink){
  var url = targetLink.attr('href');
  var stepName = targetLink.text();
  var data = {
      message: translations['IfYouChangeInfo'],
      title: translations['NavigateToStep'] + ' ' + stepName + '?',
      confirmBtnText: translations['GoToStep'] + ' ' + stepName
  };
  dynamicAlertModal(data);

  $('#confirmAlert').on('click', function(){
    window.location.href = url;
  });
}

function dynamicAlertModal(obj){
  var classes = obj.class ? obj.class : '';
    var alertModal = $('#alertModal');
    alertModal.find('.modal-body p').html(obj.message);
    alertModal.find('.modal-title').html(obj.title);
    alertModal.find('#confirmAlert').html(obj.confirmBtnText);
    alertModal.addClass(classes);
    alertModal.modal('show');
}

function hideDynamicAlertModal() {
    $('#alertModal').modal('hide');
    $('#confirmAlert').off('click');
}

function detectPageHeight(){
  if($('.dealnet-body').height() > 1000){
    $('.back-to-top-hold').show();
  }else{
    $('.back-to-top-hold').hide();
  }
}

function commonDataTablesSettings(){
  if($('#work-items-table').length){
    $('#work-items-table, .total-info').hide();

    /*$('#work-items-table').on( 'draw.dt', function () {
      $('.edit-control a').html('<svg aria-hidden="true" class="icon icon-edit"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-edit"></use></svg>');
      $('.contract-controls a.preview-item').html('<svg aria-hidden="true" class="icon icon-preview"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-preview"></use></svg>');
      $('.contract-controls a.export-item').html('<svg aria-hidden="true" class="icon icon-excel"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-excel"></use></svg>');
    });*/

    $.extend( true, $.fn.dataTable.defaults, {
      "fnInitComplete": function() {
        $('#work-items-table, .total-info').show();
        $('#work-items-table_filter input[type="search"], .dataTables_length select').removeClass('input-sm');
        customizeSelect();
      }
      });
  }
}

function resetDataTablesExpandedRows(table){
  table.rows().every( function (i) {
    var child = this.child;
    var row = this.node();
    if ( child.isShown() ) {
      child.hide();
      $(row).removeClass('parent');
    }
  } );
}

function redrawDataTablesSvgIcons(){
  $('.edit-control a').html('<svg aria-hidden="true" class="icon icon-edit"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-edit"></use></svg>');
  $('.checkbox-icon').html('<svg aria-hidden="true" class="icon icon-checked"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-checked"></use></svg>');
  $('.contract-controls a.preview-item').html('<svg aria-hidden="true" class="icon icon-preview"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-preview"></use></svg>');
  $('.contract-controls a.export-item').html('<svg aria-hidden="true" class="icon icon-excel"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-excel"></use></svg>');
}

function backToTop() {
  $("html,body").animate({scrollTop: 0}, 1000);
  return false;
};

function showLoader(loadingText) {
  var classes = loadingText ? 'hasText loader' : 'loader';
    $.loader({
        className: classes,
        content: loadingText,
        width: 101,
        height: 100
  });
}

function resetPlacehoder(elems){
  elems.removeClass('placeholder');
  setTimeout(function(){
    elems.placeholder();
  }, 0);
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

$.prototype.disableTab = function() {
  this.each(function() {
    $(this).attr('tabindex', '500');
  });
};

function customizeSelect(){
  setTimeout(function(){
    $('select').each(function(){
      var selectClasses = $(this).hasClass("dealnet-disabled-input") || $(this).hasClass("control-disabled") ? "custom-select-disabled" : "custom-select";
      if(!$(this).parents(".ui-datepicker").length && !$(this).parents(".custom-select").length && !$(this).parents(".custom-select-disabled").length){
        $(this).wrap('<div class='+selectClasses+'>');
        if(detectIE() === false){
          $(this).after('<span class="caret">');
        }
      }
    });

    $('select.dealnet-disabled-input').disableTab();
  }, 300);
}

function addIconsToFields(fields){
  var fields = fields || ($('.control-group input, .control-group textarea'));
  //var fieldParent = fields.parent('.control-group:not(.date-group):not(.control-group-pass)');
  var fieldDateParent = fields.parent('.control-group.date-group');
  var fieldPassParent = fields.parent('.control-group.control-group-pass');
  var iconCalendar = '<svg aria-hidden="true" class="icon icon-calendar"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-calendar"></use></svg>';
  var iconClearField = '<a class="clear-input"><svg aria-hidden="true" class="icon icon-remove"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-remove"></use></svg></a>';
  var iconPassField = '<a class="recover-pass-link"><svg aria-hidden="true" class="icon icon-eye"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-eye"></use></svg></a>';

  if(fieldDateParent.children('.icon-calendar').length === 0){
    fieldDateParent.append(iconCalendar);
  }

  fields.each(function(){
    var fieldParent = $(this).parent('.control-group:not(.date-group):not(.control-group-pass)');
    if(!$(this).is(".dealnet-disabled-input") && $(this).attr("type") !== "hidden"){
      if(fieldParent.children('.clear-input').length === 0){
        fieldParent.append(iconClearField);
      }
    }
  })


  if(fieldPassParent.children('.recover-pass-link').length === 0){
    fieldPassParent.append(iconPassField);
  }

  setTimeout(function(){
    fields.each(function(){
      if($(this).val().length > 0){
        $(this).siblings('.clear-input').css('display', 'block');
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
      $(this).siblings('.clear-input').css('display', 'block');
    }else{
      $(this).siblings('.clear-input').hide();
    }
  });
  fieldParent.find('.clear-input').on('click', function(){
    $(this).siblings('input, textarea').val('').change();
    $(this).hide();
  });
}

function recoverPassword(){
  var pass;
  $('.recover-pass-link').on('click', function(){
    pass = $(this).parents('.control-group-pass').find('input');
    if(pass.prop('type') == "password"){
      pass.prop('type', 'text');
    }else{
      pass.prop('type', 'password');
    }
    return false;
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

function setDeviceClasses(){
  var isMobile = {
    Android: function() {
      return navigator.userAgent.match(/Android/i);
    },
    BlackBerry: function() {
      return navigator.userAgent.match(/BlackBerry/i);
    },
    iOS: function() {
      return navigator.userAgent.match(/iPhone|iPad|iPod/i);
    },
    Opera: function() {
      return navigator.userAgent.match(/Opera Mini/i);
    },
    Windows: function() {
      return navigator.userAgent.match(/IEMobile/i);
    },
    any: function() {
      return (isMobile.Android() || isMobile.BlackBerry() || isMobile.iOS() || isMobile.Opera() || isMobile.Windows());
    }
  };

  if(isMobile.any()) {
    if(viewport().width < 768){
      $('body').addClass('mobile-device').removeClass('tablet-device')
    }else{
      $('body').addClass('tablet-device').removeClass('mobile-device')
    }
  }else{
    $('body').removeClass('mobile-device').removeClass('tablet-device')
  }
  if(isMobile.iOS()){
    $('body').addClass('ios-device');
    $('.modal').removeClass('fade').addClass('fade-on-ios');
  }else{
    $('body').removeClass('ios-device')
  }
}

function viewport() {
  var e = window, a = 'inner';
  if (!('innerWidth' in window )) {
    a = 'client';
    e = document.documentElement || document.body;
  }
  return { width : e[ a+'Width' ] , height : e[ a+'Height' ] };
}

function customDPSelect(){
  var inp = $(this);
  var selectClasses = "custom-select datepicker-select";
  if($('select.ui-datepicker-month').length && !$('.ui-datepicker-month').parents('.custom-select').length){
    $('.ui-datepicker-month')
      .wrap($('<div>', {
        class: selectClasses
      })).after('<span class="caret">');
  }
  if($('select.ui-datepicker-year').length && !$('.ui-datepicker-year').parents('.custom-select').length){
    $('.ui-datepicker-year')
      .wrap($('<div>', {
        class: selectClasses
      })).after('<span class="caret">');
  }
  if($('select.ui-datepicker-month').length){
    $('.ui-datepicker-prev, .ui-datepicker-next').hide();
  }
}
