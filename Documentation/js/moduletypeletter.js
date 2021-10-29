$(document).ready(function() {
	$('.letterbutton').click(function() {
		var elem = $(this);
		elem.toggleClass('lettertoggle');
		$('[name="' + elem.text() + '"]').toggle();
	});

	$('.allbutton').click(function() {
		var elem = $(this);
		var siblings = elem.siblings();

		if (elem.hasClass('lettertoggle')) {
			elem.removeClass('lettertoggle');
			elem.text('All');
			siblings.removeClass('lettertoggle');
			siblings.each(function() {
				$('[name="' + $(this).text() + '"]').hide();
			});
		}
		else {
			elem.addClass('lettertoggle');
			elem.text('None');
			siblings.addClass('lettertoggle');
			siblings.each(function() {
				$('[name="' + $(this).text() + '"]').show();
			});
		}
	});
});
