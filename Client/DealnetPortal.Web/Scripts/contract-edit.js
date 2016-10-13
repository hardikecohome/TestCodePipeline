$(document)
		.ready(function () {
		    $('.comment .show-comments-answers').on('click', function () {
		        $(this).toggleClass('active');
		        $(this).parents('.comment').toggleClass('active');
		        return false;
		    });


		    $('.comment .write-reply-link').on('click', function () {
		        var currComment = $(this).parents('.comment');
		        var commentForm = $('.comment-reply-form').detach();
		        commentForm.appendTo(currComment);
		        return false;
		    });
		});