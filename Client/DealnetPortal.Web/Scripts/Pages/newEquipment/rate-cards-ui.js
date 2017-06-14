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

    var slickRateCards = function() {
        if (viewport().width <= 1023) {
            $('.rate-cards-container').not('.slick-initialized').not('.one-rate-card').slick({
                infinite: false,
                dots: true,
                speed: 300,
                slidesToShow: 4,
                slidesToScroll: 4,
                responsive: [
                    {
                        breakpoint: 1024,
                        settings: {
                            slidesToShow: 2,
                            slidesToScroll: 2,
                            infinite: false
                        }
                    },
                    {
                        breakpoint: 768,
                        settings: {
                            slidesToShow: 1,
                            slidesToScroll: 1,
                            infinite: false
                        }
                    }
                ]
            });
        } else {
            if ($('.rate-cards-container').is('.slick-initialized')) {
                $('.rate-cards-container').slick("unslick");
            }
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
        slickRateCards();

        $(window).resize(function () {
            slickRateCards();
            setHeight();
        });
    });

    return {
        hide: hideRateCardBlock,
        show: showRateCardBlock,
        toggle: toggleRateCardBlock,
        highlightCard: highlightCard
    };
})