(function ($) {
    var redirectBox = $('#searchBox');
    var goButton = $('#searchButton');
    var defaultSearchTerms = redirectBox[0].value;
    var currentSearchTerms = defaultSearchTerms;

    // Ensures that the correct event is raised on the server if the <ENTER> key is pressed
    $(redirectBox).keypress(function (e) {
        if (e.keyCode == 13) {
            e.preventDefault();
            $(goButton).click();
        }
    });

    // If the search box gets focus and the value is the default keywords 
    // initially set when the page was requested the value is set to blank
    redirectBox.focus(function () {
        if (redirectBox[0].value === defaultSearchTerms) {
            redirectBox.toggleClass('init');
            redirectBox[0].value = "";
        }
    });

    // If the search box loses focus and the value is blank then this will
    // repopulate the search box value with the default keywords initially set
    // when the page was requested
    redirectBox.blur(function () {
        if (redirectBox[0].value === "") {
            if (currentSearchTerms == defaultSearchTerms) {
                redirectBox.toggleClass('init');
            }
            redirectBox[0].value = currentSearchTerms;
        }
    });
})(jQuery);