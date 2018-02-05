$(document).ready(function() {

  $(function() {
    $('select.custom-select').selectric({
      responsive: true,
    });
  });

  makeMobileNav("#mobile-navigation", ".mobile-navigation__active-item")();
  makeMobileNav("#mobile-navigation-help", ".mobile-help__toggle")();

  function makeMobileNav(id, button) {
    var idBlock = $(id);
    var buttonBlock = button;

    return function() {
      idBlock.find(buttonBlock).click(handleClick);
      $(".body-dark-overlay").click(hideMenu);

      function handleClick() {
        if(idBlock.find(".mobile-navigation__dropdown").hasClass("active")){
          hideMenu();
        } else {
          idBlock.find(".mobile-navigation__dropdown").slideDown(300, function(){
            idBlock.find(".mobile-navigation__dropdown").addClass("active");
          });
          $(".body-dark-overlay").show().fadeTo(300, 0.3);
        }
      }

      function hideMenu() {
        idBlock.find(".mobile-navigation__dropdown").slideUp(300, function(){
          idBlock.find(".mobile-navigation__dropdown").removeClass("active");
        });
        $(".body-dark-overlay").fadeOut(300);
      }
    };
  }

});
