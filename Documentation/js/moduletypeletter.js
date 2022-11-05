$(document).ready(function() {
	$('.letterbutton').click(function() {
		var elem = $(this);
		elem.toggleClass('lettertoggle');
		$('[name="' + elem.text() + '"]').toggle();
	});

	$('.allbutton').click(function() {
		var elem = $(this);
		var buttons = $('.letterbutton');

		if (elem.hasClass('lettertoggle')) {
			elem.removeClass('lettertoggle');
			elem.text('All');
			buttons.removeClass('lettertoggle');
			buttons.each(function() {
				$('[name="' + $(this).text() + '"]').hide();
			});
		}
		else {
			elem.addClass('lettertoggle');
			elem.text('None');
			buttons.addClass('lettertoggle');
			buttons.each(function() {
				$('[name="' + $(this).text() + '"]').show();
			});
		}
	});
});
