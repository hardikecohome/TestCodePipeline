$(document)
    .ready(function () {
      if(detectIE()){
        $('body').addClass('ie');
      }

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

      if($('.basic-info-cols.credit-check-info-hold .col-md-6').length % 2 !== 0){
        $('.summary-info-hold #contact-info-form.credit-check-info-hold').addClass('shift-to-basic-info');
      }

      $('.dealnet-disabled-input').each(function(){
        $(this).attr('type', 'hidden');
        var inpValue = $(this).is('select')? $(this).find("option:selected").text() : $(this).is('textarea')? $(this).text() : $(this).val();
        $(this).after($('<div/>',{
          class: "dealnet-disabled-input dealnet-disabled-input-value",
          text: inpValue
        }));
      });

      $(window).on('scroll', function(){
        detectPageHeaight()
      }).on('resize', function(){
        detectPageHeaight();
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



      addIconsToFields();
      toggleClearInputIcon();
      customizeSelect();
      commonDataTablesSettings();
    });

function documentsColHeight(){
  var columns = $('.report-documents-list .document-col');
  console.log(columns.find('.documents-inner').height());
  columns.find('.dealnet-credit-check-section').css('min-height', columns.find('.documents-inner').height());
}

function navigateToStep(targetLink){
  var url = targetLink.attr('href');
  var stepName = targetLink.text();

  var message = "If you change Home Owner Information you will have to pass Credit Check step again"
  $('#alertModal').find('.modal-body p').html(message);
  $('#alertModal').find('.modal-title').html('Navigate to step '+stepName+'?');
  $('#alertModal').find('#confirmAlert').html('Go to Step '+stepName);
  $('#alertModal').modal('show');
  $('#confirmAlert').on('click', function(){
    window.location.href = url;
  });
}

function detectPageHeaight(){
  if($('.dealnet-body').height() > 1000){
    $('.back-to-top-hold').show();
  }else{
    $('.back-to-top-hold').hide();
  }
}

function commonDataTablesSettings(){
  $.extend( true, $.fn.dataTable.defaults, {
    "fnInitComplete": function(oSettings, json) {
      $('#work-items-table_filter input[type="search"], .dataTables_length select').removeClass('input-sm');
      customizeSelect();
    }
  } );
}

function backToTop() {
  $("html,body").animate({scrollTop: 0}, 1000);
  return false;
};

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

function customizeSelect(){
  setTimeout(function(){
    $('select').each(function(){
      var selectClasses = $(this).hasClass("dealnet-disabled-input") ? "custom-select-disabled" : "custom-select";
      if(!$(this).parents(".ui-datepicker").length && !$(this).parents(".custom-select").length && !$(this).parents(".custom-select-disabled").length){
        $(this).wrap('<div class='+selectClasses+'>');
        if(detectIE() === false){
          $(this).after('<span class="caret">');
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
  var iconCalendar = $('<svg aria-hidden="true" class="icon icon-calendar"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-calendar"></use></svg>');
  var iconClearField = $('<a class="clear-input"><svg aria-hidden="true" class="icon icon-remove"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-remove"></use></svg></a>');
  var iconPassField = $('<svg aria-hidden="true" class="icon icon-eye"><use xlink:href="'+urlContent+'Content/images/sprite/sprite.svg#icon-eye"></use></svg>');
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