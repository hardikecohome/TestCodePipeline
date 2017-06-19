module.exports('rate-cards-ui', function (require) {

    var showRateCardBlock = function () {
        $('#rateCardsBlock').addClass('opened')
                            .removeClass('closed');

        $('#loanRateCardToggle').find('i.glyphicon')
          .removeClass('glyphicon-chevron-right')
          .addClass('glyphicon-chevron-down');


        if (!$('#paymentInfo').hasClass('hidden')) {
            $('#paymentInfo').css('display', 'none');
            $('#paymentInfo').addClass('hidden');
        }
    }

    var hideRateCardBlock = function () {
        $('#rateCardsBlock').removeClass('opened')
                            .addClass('closed');

        $('#loanRateCardToggle').find('i.glyphicon')
          .removeClass('glyphicon-chevron-down')
          .addClass('glyphicon-chevron-right');

        if ($('#paymentInfo').hasClass('hidden')) {
            $('#paymentInfo').css('display', 'block');
            $('#paymentInfo').removeClass('hidden');
        }
    }

    var toggleRateCardBlock = function (isOpenCondition) {
        isOpenCondition === true ? showRateCardBlock() : hideRateCardBlock();
    }

    var setEqualHeightRows = function(row) {
        var maxHeight = 0;
        row.each(function () {
            if ($(this).height() > maxHeight) {
                maxHeight = $(this).height();
            }
        });
        row.height(maxHeight);
    }

    var updateEquipmentCosts = function (agreementType) {
        if (agreementType === "Loan") {
            $(".equipment-cost").each(function () {
                var input = $(this);
                input.prop("disabled", false).parents('.equipment-cost-col').show();
                input[0].form && input.rules("add", "required");
            });
            $(".monthly-cost").each(function () {
                var input = $(this);
                input.prop("disabled", true).parents('.monthly-cost-col').hide();
                input[0].form && input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
        } else {
            $(".equipment-cost").each(function () {
                var input = $(this);
                input.prop("disabled", true).parents('.equipment-cost-col').hide();
                input[0].form && input.rules("remove", "required");
                input.removeClass('input-validation-error');
                input.next('.text-danger').empty();
            });
            $(".monthly-cost").each(function () {
                var input = $(this);
                input.prop("disabled", false).parents('.monthly-cost-col').show();
                input[0].form && input.rules("add", "required");
            });
        }
    }

    var setHeight = function() {
        setEqualHeightRows($(".equal-height-row-1"));
        setEqualHeightRows($(".equal-height-row-2"));
        setEqualHeightRows($(".equal-height-row-3"));
        setEqualHeightRows($(".equal-height-row-4"));
    }

    var onAgreemntSelect = function () {
        var agreementType = $(this).find("option:selected").text();
        if (agreementType === "Loan") {
            //If loan is chosen
            setHeight();

            if (!$("#submit").hasClass('disabled') && $('#rateCardsBlock').find('div.checked').length === 0) {
                $('#submit').addClass('disabled');
                $('#submit').parent().popover();
            }

            $('#loanRateCardToggle, .downpayment-row').show();
            $('.rental-element').hide();

            if ($('#rateCardsBlock').find('div.checked').length) {
                $('#paymentInfo').show();
            } else {
                $('#rateCardsBlock').addClass('opened')
                  .removeClass('closed');
            }
            $('#equipment-form').off('change', '.monthly-cost');
        } else {
            //If rental is chosen
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            }
            setHeight();
            $('.rental-element').show();
            $('.loan-element, .downpayment-row').hide();

            $('#equipment-form').on('change', '.monthly-cost', function () {
                var value = 0;
                $.each($('.monthly-cost'), function () {
                    var $this = $(this);
                    if ($this[0].form) {
                        var temp = parseFloat($this.val());
                        if (!isNaN(temp))
                            value += temp;
                    }
                });
                $('#total-monthly-payment').val(value).change();
            });
        }
        updateEquipmentCosts(agreementType);
    }

    var highlightCard = function () {

        $(this).parents('.rate-card')
            .addClass('checked')
            .parents('li')
            .siblings()
            .find('div')
            .removeClass('checked');

        return false;
    }

    $(document).ready(function () {
        if (onlyCustomCard) {
            $('#rateCardsBlock').addClass('one-rate-card');
        }

        $('#typeOfAgreementSelect').on('change', onAgreemntSelect).change();

        setHeight();
        carouselRateCards();

        $(window).resize(function () {
            carouselRateCards()
            setHeight();
        });
    });

    return {
        hide: hideRateCardBlock,
        show: showRateCardBlock,
        toggle: toggleRateCardBlock,
        highlightCard: highlightCard
    };
});


function  carouselRateCards(){
    var windowWidth = $(window).width();
    var paginationItems;
    var targetSlides;
    if (windowWidth >= 1024) {
        paginationItems = 4;
        targetSlides = 0;
    } else if (windowWidth >= 768) {
        paginationItems = 2;
        targetSlides = 2;
    }else {
        paginationItems = 1;
        targetSlides = 1;
    }
    var jcarousel = $('.rate-cards-container:not(".one-rate-card") .jcarousel');

    jcarousel
      .on('jcarousel:reload jcarousel:create', function () {
          var carousel = $(this),
            width = carousel.innerWidth(),
            windowWidth = $(window).width();

          if (windowWidth >= 1024) {
              width = width / 4;
          } else if (windowWidth >= 768) {
              width = width / 2;
          }else {
              width = width / 1;
          }

          carousel.jcarousel('items').css('width', Math.ceil(width) + 'px');
      })
      .jcarousel();

    $('.jcarousel-control-prev')
      .jcarouselControl({
          target: '-='+targetSlides
      });

    $('.jcarousel-control-next')
      .jcarouselControl({
          target: '+='+targetSlides
      });

    $('.jcarousel-pagination')
      .on('jcarouselpagination:active', 'a', function() {
          $(this).addClass('active');
          if($(this).is(':first-child')){
              $('.jcarousel-control-prev').addClass('disabled');
          }else{
              $('.jcarousel-control-prev').removeClass('disabled');
          }
          if($(this).is(':last-child')){
              $('.jcarousel-control-next').addClass('disabled');
          }else{
              $('.jcarousel-control-next').removeClass('disabled');
          }

      })
      .on('jcarouselpagination:inactive', 'a', function() {
          $(this).removeClass('active');
      })
      .on('click', function(e) {
          e.preventDefault();
      })
      .jcarouselPagination({
          perPage: paginationItems,
          item: function(page) {
              return '<a href="#' + page + '">' + page + '</a>';
          }
      });



}