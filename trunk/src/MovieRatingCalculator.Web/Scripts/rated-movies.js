$(document).ready(function () {

    $(".enable-rating-link").click(function () {
        $(".editable").show();
        $(".readonly").hide();
        return false;
    });

    $(".movie-rate-list").change(function () {
        var rateElem = $(this);
        var spanMessage = $(rateElem).siblings("#message-text");
        var movieId = $(rateElem).siblings("#movie-id").val();
        var link = $(rateElem).parent().parent().find(".rating-disrtibution-link");
        var movieRate = {
            movieId: movieId,
            rating: $(rateElem).val(),
            doUpdateMovieInfo: true
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
                RemoveBarChart("chart-ratings-distribution-" + movieId);
                ShowRatingDistributionLink(link);

                if (data.result == "success-add") {
                    ShowMessage(spanMessage, "Rating saved");
                    $(rateElem).parent().parent().find("#total-rates-number").text(data.totalNumber);
                    $(rateElem).parent().parent().find("#users-same-rating").text(data.sameRatingNumber);

                    /* $(rateElem).parent().parent().find("a").removeClass("gray-out");
                    $(rateElem).parent().siblings(".movie-info").removeClass("gray-out");*/
                }
                if (data == "success-delete") {
                    ShowMessage(spanMessage, "Rating removed");

                    var totalRates = $(rateElem).parent().parent().find("#total-rates-number");
                    $(totalRates).text(parseInt($(totalRates).text() - 1));
                    $(rateElem).parent().parent().find("#users-same-rating").text("-");

                    /* $(rateElem).parent().parent().find("a").addClass("gray-out");
                    $(rateElem).parent().siblings(".movie-info").addClass("gray-out");*/
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

    //jqplot, shows rating distribution plot for every rated movie
    $(".rating-disrtibution-link").click(function () {
        var movieId = $(this).parent().parent().find("#movie-id").val();
        var rating = $(this).parent().parent().find(".movie-rate-list").val();
        var link = this;
        var chartElemId = "chart-ratings-distribution-" + movieId;

        if ($(this).hasClass("hide")) {
            ShowRatingDistributionLink(this);
            RemoveBarChart(chartElemId);
            return;
        }

        $.ajax({
            url: $("#rating-distribution-url").val(),
            type: "post",
            dataType: "json",
            data: { movieId: movieId },
            beforeSend: function () {
                $('#loadingDiv').show();
            },
            success: function (data) {
                $('#loadingDiv').hide();
                $("#" + chartElemId).removeClass("hidden");
                HideRatingDistributionLink(link);
                
                //
                var maxCount = 0;
                for (var k = 0; k < data.length; k++) {
                    if (data[k] > maxCount) {
                        maxCount = data[k];
                    }
                }
                //
                
                var ratingsPoints = [];
                var currentRatingPoint = [];
                for (var i = 0; i < data.length; i++) {
                    if (i + 1 != parseInt(rating)) {
                        ratingsPoints.push([i + 1, data[i]]);
                        currentRatingPoint.push([i + 1, 0]);
                    }
                    else {
                        ratingsPoints.push([i + 1, 0]);
                        currentRatingPoint.push([i + 1, data[i]]);
                    }
                }
                CreateBarChart(chartElemId, ratingsPoints, currentRatingPoint, maxCount);
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
    $(element).fadeIn(500);
    $(element).fadeOut(1000);
}

function ShowRatingDistributionLink(link) {
    $(link).removeClass("hide");
    $(link).addClass("show");
    $(link).text("Show rating distribution");
}

function HideRatingDistributionLink(link) {
    $(link).removeClass("show");
    $(link).addClass("hide");
    $(link).text("Hide rating distribution");
}

function RemoveBarChart(chartElemId) {
    $("#" + chartElemId).empty();
    $("#" + chartElemId).addClass("hidden");
}

function CreateBarChart(chartElemId, ratingsPoints, currentRatingPoint, maxRatings) {
    // Can specify a custom tick Array.
    // Ticks should match up one for each y value (category) in the series.
    //var ticks = ['1', '2', '3', '4', '5', '6', '7', '8', '9', '10'];
    var tickInt = parseInt(maxRatings / 10) * 2;
    if (tickInt == 0) {
        tickInt = 1;
    }
    var plot = $.jqplot(chartElemId, [ratingsPoints, currentRatingPoint], {
        seriesDefaults: {
            renderer: $.jqplot.BarRenderer,
            rendererOptions: { fillToZero: true, barMargin: 30 }
        },
        axes: {
            // Use a category axis on the x axis and use our custom ticks.
            xaxis: {
                renderer: $.jqplot.CategoryAxisRenderer
                //ticks : ticks
            },
            // Pad the y axis just a little so bars can get close to, but
            // not touch, the grid boundaries.  1.2 is the default padding.
            yaxis: {
                pad: 1.05,
                min: 0,
                tickInterval: tickInt,
                tickOptions: {
                    formatString: '%d'
                }
            }
        }
    });
}