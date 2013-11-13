$(document).ready(function () {
    
    $(".movie-div").each(function () {
        var rateInfo = $(this).children(".movie-rate-info");
        if (rateInfo.length > 0) {
            var rating = $(rateInfo).children(".movie-rate-list");
            if ($(rating).val() != "0") {
                var movieInfo = $(this).children(".movie-info");
                $(movieInfo).addClass("gray-out");
                $(movieInfo).find("a").addClass("gray-out");
            }
        }
    });

    $(".movie-rate-list").change(function () {
        var rateElem = $(this);
        var spanMessage = $(rateElem).siblings("#message-text");
        var movieId = $(rateElem).siblings("#movie-id").val();
        var movieRate = {
            movieId: movieId,
            rating: $(rateElem).val()
        };
        $.ajax({
            url: $("#save-ratings-url").val(),
            type: "post",
            dataType: "json",
            data: movieRate,
            beforeSend: function () {
                $(spanMessage).hide();
                $('#loadingDiv').show();
            },
            success: function (data) {
                $('#loadingDiv').hide();
                if (data == "success-add") {
                    ShowMessage(spanMessage, "Rating saved");
                    $(rateElem).parent().parent().find("a").addClass("gray-out");
                    $(rateElem).parent().siblings(".movie-info").addClass("gray-out");

                }
                if (data == "success-delete") {
                    ShowMessage(spanMessage, "Rating removed");
                    $(rateElem).parent().parent().find("a").removeClass("gray-out");
                    $(rateElem).parent().siblings(".movie-info").removeClass("gray-out");
                }
                if (data !== "error-unathorized-access") {
                    updateProgressBar();
                }
            },
            error: function () {
            },
            complete: function () {
                $('#loadingDiv').hide();
            },
            cache: false
        });
    });
});

function ShowMessage(element, message) {
    $(element).text(message);
    $(element).fadeIn(1000);
    $(element).fadeOut(1000);
}

$("#year-filter-field").live("keypress", function (event) {
    // Allow: backspace, delete, tab, escape, and enter
    if (event.keyCode == 127 || event.keyCode == 8 || event.keyCode == 9 || event.keyCode == 27 || event.keyCode == 13 ||
    // Allow: Ctrl+A
            (event.keyCode == 65 && event.ctrlKey === true) ||
    // Allow: home, end, left, right
            (event.keyCode <= 31)) {
        // let it happen, don't do anything
        return;
    }
    else {
        if (event.which < 48 || event.which > 57) {
            event.preventDefault();
        }
    }
});