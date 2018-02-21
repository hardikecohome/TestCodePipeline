$(document).ready(function() {
  var node = $("#mobile-navigation");
  node.find(".mobile-navigation__active-item").click(handleMobileNavigation);
  $(".body-dark-overlay").click(handleMobileNavigation);

  function handleMobileNavigation(){
    if(node.find(".mobile-navigation__dropdown").hasClass("active")){
      node.find(".mobile-navigation__dropdown").slideUp(300, function(){
        node.find(".mobile-navigation__dropdown").removeClass("active");
      });
      $(".body-dark-overlay").fadeOut(300);

    } else {
      node.find(".mobile-navigation__dropdown").slideDown(300, function(){
        node.find(".mobile-navigation__dropdown").addClass("active");

      });
      $(".body-dark-overlay").show().fadeTo(300, 0.3);
    }
  }

});
