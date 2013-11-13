$(document).ready(function () {
    updateProgressBar();
});

function updateProgressBar()
{
    // Check if element exists
    if ($("#user-rating-status-url").length > 0) {
        $.ajax({
            url: $("#user-rating-status-url").val(),
            type: "post",
            dataType: "json",
            success: function (data) {
                var numberOfMoviesRated = data.NumberOfMoviesRated;
                var minMoviesToRate = data.MinMoviesToRate;

                // NOTE: This value should correspond with the #progress-bar width in the CSS definition
                var minProgressBarWidth = 200;
                var coef = minProgressBarWidth / minMoviesToRate;

                if (numberOfMoviesRated >= minMoviesToRate) {
                    $("#user-rating-status").text("Total movies rated: " + numberOfMoviesRated + ". Feel free to rate more!");
                    $("#progress-bar-container").hide();

                    $("#progress-section").removeClass("progress-section");
                    $("#progress-section").addClass("progress-section-completed");
                } else {
                    $("#user-rating-status").text("Total movies rated: " + numberOfMoviesRated + " of " + minMoviesToRate);

                    if (numberOfMoviesRated > 0) {
                        $("#filled-in-div").css("width", (coef * numberOfMoviesRated) + "px");
                        $("#rated-films-count").text(numberOfMoviesRated);
                        $("#filled-in-div").show();
                    } else {
                        $("#filled-in-div").hide();
                    }
                    $("#progress-bar-container").show();
                }
            },
            cache: false
        });
    }
}