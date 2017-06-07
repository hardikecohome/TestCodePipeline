module.exports('rate-cards-ui', function() {
    $(document).ready(function () {
        $('#loanRateCardToggle').click(function () {
            if (!$('#rateCardsBlock').is(':visible')) {
                $('#rateCardsBlock').show('slow',
                    function () {
                        $('#loanRateCardToggle').find('i.glyphicon')
                            .removeClass('glyphicon-chevron-right')
                            .addClass('glyphicon-chevron-down');
                    });
            } else {
                $('#rateCardsBlock').hide('slow',
                    function () {
                        $('#loanRateCardToggle').find('i.glyphicon')
                            .removeClass('glyphicon-chevron-down')
                            .addClass('glyphicon-chevron-right');
                    });
            }
        });

        $('.btn-select-card').on('click',
            function () {
                return false;
            });
        $('#typeOfAgreementSelect').on('change',
            function () {
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
            }).change();

        setHeight();
        slickRateCards();
        $(window).resize(function () {
            slickRateCards();
            setHeight();
        });

        function setEqualHeightRows(row) {
            var maxHeight = 0;
            row.each(function () {
                if ($(this).height() > maxHeight) {
                    maxHeight = $(this).height();
                }
            });
            row.height(maxHeight);
        }

        function setHeight() {
            setEqualHeightRows($(".equal-height-row-1"));
            setEqualHeightRows($(".equal-height-row-2"));
            setEqualHeightRows($(".equal-height-row-3"));
            setEqualHeightRows($(".equal-height-row-4"));
        }

        function slickRateCards() {
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
    });
})