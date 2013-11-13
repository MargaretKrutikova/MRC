$(document).ready(function () {
    $('nav ul li a').each(function () {
        $(this).removeClass("selected-link");
    });

    var act = $("#act").val();
    if (act) {
        if (act == "suggest") {
            SelectMenuItem("suggest");
        } else if (act == "search") {
            SelectMenuItem("search");
        }
    } else {
        var current = window.location.pathname.trim();
        if (current != "/") {
            SelectMenuItem(current);
        }
    }
});
function SelectMenuItem(searchText) {
    $('nav ul li a').each(function () {
        if ($(this).attr('href').indexOf(searchText.trim()) >= 0) {
            $(this).addClass("selected-link");
        }
    });
}