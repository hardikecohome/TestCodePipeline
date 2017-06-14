module.exports('rate-cards-ui', function () {

    var showRateCardBlock = function () {
        $('#rateCardsBlock').show('slow',
            function () {
                $('#loanRateCardToggle').find('i.glyphicon')
                    .removeClass('glyphicon-chevron-right')
                    .addClass('glyphicon-chevron-down');
            });
    }

    var hideRateCardBlock = function () {
        $('#rateCardsBlock').hide('slow',
            function () {
                $('#loanRateCardToggle').find('i.glyphicon')
                    .removeClass('glyphicon-chevron-down')
                    .addClass('glyphicon-chevron-right');
            });
    }

    var toggleRateCardBlock = function(isOpenCondition) {
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

    var setHeight = function() {
        setEqualHeightRows($(".equal-height-row-1"));
        setEqualHeightRows($(".equal-height-row-2"));
        setEqualHeightRows($(".equal-height-row-3"));
        setEqualHeightRows($(".equal-height-row-4"));
    }

    var onAgreemntSelect = function () {
        if ($(this).find("option:selected").text() === "Loan") {
            //If loan is chosen
            setHeight();

            if (!$("#submit").hasClass('disabled') && $('#rateCardsBlock').find('div.checked').length === 0) {
                $('#submit').addClass('disabled');
            }

            $('#loanRateCardToggle').show();
            $('.rental-element').hide();
            $('.loan-element').show();
        } else {
            //If rental is chosen
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
            }
            setHeight();
            $('#loanRateCardToggle').hide();
            $('.rental-element').show();
            $('.loan-element').hide();
        }
    }

    var highlightCard = function() {
        $(this).parents('.rate-card').addClass('checked').siblings().removeClass('checked');

        return false;
    }

    $(document).ready(function () {
        if (onlyCustomCard) {
            $('#rateCardsBlock').addClass('one-rate-card');
        }
        $('#loanRateCardToggle').click(function () {
            toggleRateCardBlock(!$('#rateCardsBlock').is(':visible'));
        });

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
        console.log('adfads')
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