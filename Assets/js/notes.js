var notes = notes || {};
notes = (function($){
	$('[data-toggle=popover]').popover();

	$('[data-href]').click(function() {
		var href = $(this).data('href');
		location.href = href;
	});
	
	$('#selectAll').click(function () {
		$('input[name=id][type=checkbox]').prop('checked', function() {
			if ($('#selectAll').is(':checked')) {
				return true;
			}
			return false;
		});
	});

	$('#trashSubmit').click(function() {
		var id = $('#trash').data('id');
		$.ajax({
			type: "POST",
			url: '/note/remove/' + id
		}).done(function() {
			location.href = '/';
		}).fail(function() {
			alert('Failed to delete note.');
		});
	});

	$('#notebook').on('hidden.bs.modal', function() {
        $('#notebookValue').val($('#notebook').data('value'));
    });

	$('#notebookRestore').click(function() {
		$('#notebookValue').val($('#notebook').data('value'));
	});

	$('#notebookDelete').click(function() {
		$('#notebookValue').val('');
	});

	$('#notebookSubmit').click(function() {
		var id = $('#notebook').data('id');

		var d = {
			id: id,
			notebook: $('#notebookValue').val()
		};

		$.ajax({
			type: "POST",
			url: '/note/notebook/',
			dataType: 'json',
			data: d
		}).done(function() {
			location.reload();
		}).fail(function() {
			alert('Failed to write notebook.');
		});
	});

	$('#tags').on('hidden.bs.modal', function() {
        $('#tagsValue').val($('#tags').data('value'));
    });

	$('#tagsRestore').click(function() {
		$('#tagsValue').val($('#tags').data('value'));
	});

	$('#tagsDelete').click(function() {
		$('#tagsValue').val('');
	});

	$('#tagsSubmit').click(function() {
		var id = $('#tags').data('id');

		var d = {
			id: id,
			tags: $('#tagsValue').val()
		};

		$.ajax({
			type: "POST",
			url: '/note/tags/',
			dataType: 'json',
			data: d
		}).done(function() {
			location.reload();
		}).fail(function() {
			alert('Failed to write tags.');
		});
	});

	function getFormData($form){
		var unindexed_array = $form.serializeArray();
		var indexed_array = {};

		$.map(unindexed_array, function(n, i){
			indexed_array[n['name']] = n['value'];
		});

		return indexed_array;
	}

	$('#saveContinue').click(function() {
		if($('#note-form').valid()) {
			var data = getFormData($('#note-form'));
			$.ajax({
				type: "POST",
				url: '/note/edit/',
				dataType: 'json',
				data: data
			}).done(function(d) {
				if(d.success) {
					$('#id').val(d.id);
					
					$('#result').html('<div class="alert alert-success"><button type="button" class="close">×</button>Successfully saved note!</div>');
					window.setTimeout(function() {
						$(".alert").fadeTo(500, 0).slideUp(500, function(){
                    		$(this).remove(); 
                		});
            		}, 5000);

          			$('.alert .close').on("click", function(e){
                		$(this).parent().fadeTo(500, 0).slideUp(500);
             		});

				} else {
					$('#result').html('<div class="alert alert-danger"><button type="button" class="close">×</button>Failed to save note!</div>');

          			$('.alert .close').on("click", function(e){
                		$(this).parent().fadeTo(500, 0).slideUp(500);
             		});
				}
			}).fail(function() {
				alert('Failed to save note.');
			});
		}
	});

	$('#preview').click(function() {
		if($('#note-form').valid()) {
			if($('#editorPane').is(':visible'))
			{
				var data = getFormData($('#note-form'));
				$.ajax({
					type: "POST",
					url: '/note/preview/',
					dataType: 'json',
					data: data
				}).done(function(d) {
					$('#previewPane').html(d.content);
					$('#preview').text('Return to edit');

					$('#editorPane').toggle();
					$('#previewPane').toggle();
				});
			}
			else
			{
				$('#preview').text('Preview');

				$('#editorPane').toggle();
				$('#previewPane').toggle();
			}
		}
	});

	$.validator.setDefaults({
		highlight: function(element) {
			$(element).closest('.form-group').addClass('has-error');
		},
		unhighlight: function(element) {
			$(element).closest('.form-group').removeClass('has-error');
		}
	});

	$.validator.addMethod(
	    "regex",
	    function (value, element, regexp) {
	    	return this.optional(element) || value.match(regexp);
	    },
	    "This field has the wrong format."
	);

	$.validator.addMethod(
		'nowhitespace',
		function (value, element) {
			return this.optional(element) || value.trim() != ''
		},
		'This field is required.'
	);

	$("#note-form").validate({
		rules: {
			'title': {
				required: true,
				nowhitespace: true
			},
			'content': {
				required: true,
				nowhitespace: true
			}
		}
	});

	$("#settings-form").validate({
		rules: {
			'items': {
				required: true,
				nowhitespace: true,
				min: 1,
				max: 100
			},
			'language': {
				required: true,
				nowhitespace: true,
				regex: 'en|de'
			}
		}
	});

	$("#security-form").validate({
		rules: {
			'oldpassword': {
				required: true,
				minlength: 8
			},
			'newpassword': {
				required: true,
				minlength: 8
			},
			'confirmpassword': {
				required: true,
				minlength: 8,
				equalTo: '#newpassword'
			}
		}
	});

	$("#account-form").validate({
		rules: {
			'username': {
				required: true,
				email: true
			},
			'password': {
				required: true,
				minlength: 8
			}
		}
	});

}(jQuery));