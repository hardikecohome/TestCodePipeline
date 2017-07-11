module.exports('rate-cards-ui', function (require) {

    var state = require('state').state;

    var showRateCardBlock = function () {
        $('#rateCardsBlock').addClass('opened')
                            .removeClass('closed');
        setTimeout(setHeight(), 500);

        $('#loanRateCardToggle').find('i.glyphicon')
          .removeClass('glyphicon-chevron-down')
          .addClass('glyphicon-chevron-up');


        if (!$('#paymentInfo').hasClass('hidden')) {
            $('#paymentInfo').addClass('hidden');
        }
    }

    var hideRateCardBlock = function () {
        $('#rateCardsBlock').removeClass('opened')
                            .addClass('closed');

        $('#loanRateCardToggle').find('i.glyphicon')
          .removeClass('glyphicon-chevron-up')
          .addClass('glyphicon-chevron-down');

        if ($('#paymentInfo').hasClass('hidden')) {
            $('#paymentInfo').removeClass('hidden');
        }
    }

    var toggleRateCardBlock = function (isOpenCondition) {
        isOpenCondition === true ? showRateCardBlock() : hideRateCardBlock();
    }

    var setEqualHeightRows = function(row) {
        var maxHeight = 0;
        row.each(function () {
            if ($(this).children().eq(0).outerHeight(true) > maxHeight) {
                maxHeight = $(this).children().eq(0).outerHeight(true);
            }
        });
	      if(row.children().eq(0)){

	      }
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
        setEqualHeightRows($(".equal-height-row-5"));


        setEqualHeightRows($(".equal-height-label-1"));
    }

    var onAgreemntSelect = function () {
        var agreementType = $(this).find("option:selected").text();
        if (agreementType === "Loan") {
            //If loan is chosen
            setHeight();
            if (!$("#submit").hasClass('disabled') && $('#rateCardsBlock').find('div.checked').length === 0) {
                if (!state.onlyCustomRateCard) {
                    $('#submit').addClass('disabled');
                    $('#submit').parent().popover();
                }
            }

            $('#loanRateCardToggle, .loan-element, .downpayment-row').show();
            $('.rental-element').hide();

            if (state.onlyCustomRateCard || $('#rateCardsBlock').find('div.checked').length) {
                toggleRateCardBlock(false);
            } else {
                toggleRateCardBlock(true);
            }
        } else {
            //If rental is chosen
            if ($("#submit").hasClass('disabled')) {
                $('#submit').removeClass('disabled');
                $('#submit').parent().popover('destroy');
            }
            setHeight();
            $('.rental-element').show();
            $('#loanRateCardToggle, .loan-element, .downpayment-row').hide();
            $('#rateCardsBlock').removeClass('opened').addClass('closed');
            if (!$('#paymentInfo').hasClass('hidden')) {
                $('#paymentInfo').addClass('hidden');
            }
        }
        updateEquipmentCosts(agreementType);
    }

    var highlightCardBySelector = function (selector) {
        if (!state.onlyCustomRateCard) {
            $(selector).parents('.rate-card')
                .addClass('checked')
                .parents('li')
                .siblings()
                .find('div')
                .removeClass('checked');
        }
        return false;
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

    var togglePromoLabel = function (option) {
        var isPromo = state[option].IsPromo;
        if (isPromo) {
            if ($('#' + option + 'Promo').is('.hidden')) {
                $('#' + option + 'Promo').removeClass('hidden');
            }
        } else {
            if (!$('#' + option + 'Promo').is('.hidden')) {
                $('#' + option + 'Promo').addClass('hidden');
            }
        }
    }

    $(document).ready(function () {
        if (state.onlyCustomRateCard) {
            $('#rateCardsBlock').addClass('one-rate-card');
        }

        $('#typeOfAgreementSelect').on('change', onAgreemntSelect).change();

        setHeight();
        carouselRateCards();

        $(window).resize(function () {
            carouselRateCards();
            setHeight();
        });

		    $('.link-over-notify').popover({
			    template: '<div class="popover customer-loan-popover" role="tooltip"><h3 class="popover-title"></h3><div class="popover-content"></div></div>',
			    placement: 'top',
			    trigger: $('body').is('.tablet-device') || $('body').is('.mobile-device') ? 'click' : 'hover',
			    content: '',
		    }).on('shown.bs.popover', function () {
			    if($('body').is('.tablet-device') || $('body').is('.mobile-device')){
				    $(this).parents('div[class*="equal-height-row"]').addClass('row-auto-height');
			    }
		    }).on('hide.bs.popover', function () {
			    if($('body').is('.tablet-device') || $('body').is('.mobile-device')){
				    $(this).parents('div[class*="equal-height-row"]').removeClass('row-auto-height');
			    }
		    });
    });

    return {
        hide: hideRateCardBlock,
        show: showRateCardBlock,
        toggle: toggleRateCardBlock,
        highlightCard: highlightCard,
        togglePromoLabel: togglePromoLabel,
        highlightCardBySelector: highlightCardBySelector
    };
});


function carouselRateCards(){
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

    if (state.onlyCustomRateCard) {
        return;
    }

    var jcarousel = $('.rate-cards-container:not(".one-rate-card") .jcarousel');
		var carouselItemsToView = viewport().width >= 768 && viewport().width < 1024 ? 2 : viewport().width < 768 ? 1 : 4;
		jcarousel
      .on('jcarousel:reload jcarousel:create', function () {
          var carousel = $(this),
            carouselWidth = carousel.innerWidth(),
	          width = carouselWidth / carouselItemsToView;

          carousel.jcarousel('items').css('width', Math.ceil(width) + 'px');
      }).jcarousel();

			if(viewport().width < 1024){
				jcarousel.swipe({
					//Generic swipe handler for all directions
					swipe:function(event, direction, distance, duration, fingerCount, fingerData) {
						$('.link-over-notify').each(function(){
							if($(this).attr('aria-describedby')){
								$(this).click();
							}
						});

						if(direction === "left"){
							jcarousel.jcarousel('scroll', '+='+carouselItemsToView);
						} else if(direction === "right"){
							jcarousel.jcarousel('scroll', '-='+carouselItemsToView);
						} else {
							event.preventDefault();
						}
					},
					excludedElements: "button, input, select, textarea, .noSwipe, a",
					threshold: 50,
					allowPageScroll: "auto",
					triggerOnTouchEnd: false
				});
			}


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