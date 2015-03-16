var Ajax = (function () {
    
    //A set of common http status code along with a friendly error message
    var statusErrorMap = {
        '400': "Server understood the request but request content was invalid.",
        '401': "You do not have access to the requested resource.",
        '403': "You do not have access to the requested resource.",
        '404': "The requested service could not be found.",
        '500': "Internal Server Error.",
        '503': "Service Unavailable."
    };


    var ajaxMessage = "";
    var ajaxModal = null;
    var disableAjaxModal = false;

    //Internal function used to block the body of the page with an overlay during an ajax request
    var ajaxBlock = function () {
        if (ajaxModal == null) {
            var blah = $('<div id="AjaxLoaderDiv"><p><img src="/Content/images/ajax-loader.gif" alt="Loader Gif"/> ' + (ajaxMessage != "" ? ajaxMessage : 'Please Wait') + '</p> </div>');
            $.blockUI({
                // message: '<p><img src="/Content/images/ajax-loader.gif" alt="Loader Gif"/>' + (ajaxMessage != "" ? ajaxMessage : 'Please Wait') + '</p>',
                message: blah,
                css: {
                    border: 'none',
                    background: 'none'
                },
                fadeIn: 700,
                fadeOut: 700
            });
        } else {
            $.blockUI(ajaxModal);
        }
    };

    //Internal function used with BlockUI to remove the background overlay when an ajax request is complete
    var ajaxUnblock = function() {
           setTimeout(function() { $.unblockUI(); }, 500);
      //  $.unblockUI();
    };

    //Escapes Html. Used for text entered by users which will be sent to the server via Ajax calls.
    var encodeHtml = function (textToEncode) {
        var encodedHtml;
        encodedHtml = escape(textToEncode);
        encodedHtml = encodedHtml.replace(/\//g, "%2F");
        encodedHtml = encodedHtml.replace(/\?/g, "%3F");
        encodedHtml = encodedHtml.replace(/=/g, "%3D");
        encodedHtml = encodedHtml.replace(/&/g, "%26");
        encodedHtml = encodedHtml.replace(/@/g, "%40");

        return encodedHtml;
    };

    //builds a modal, loading the modal html from the server via an .load()
   /* var buildModal = function (dialog, buttons, title, width, height) {
      
        var newDialog = $("<div></div>");
        
        newDialog.dialog({
            modal: true,
            title: title,
            width: width,
            height: height,
            buttons:  buttons ,
            overlay: { opacity: 0.7, background: "black" },
            open: function() {  $(this).load(routes.DIALOG(dialog)); },
            close: function () { $(this).remove(); }
        });
    };*/
    
    //builds a modal, loading the modal html from the server via an .load() and allowing a callback function to be executed as the modal is loaded.
   /* var buildModalCallback = function (dialog, callback, buttons, title, width, height) {

        var newDialog = $("<div></div>");

        newDialog.dialog({
            modal: true,
            title: title,
            width: width,
            height: height,
            buttons: buttons,
            overlay: { opacity: 0.7, background: "black" },
            open: function () { $(this).load(routes.DIALOG(dialog)); callback; },
            close: function () { $(this).remove(); }
        });
    };*/
    
    //Displays a simple message as modal popup i.e. a success message.
    var buildMessageModal = function (message, buttons, title) {

        var newDialog = $("<div class='modal fade'>" +
                "<div class='modal-dialog'>" +
                    "<div class='modal-content'>" +
                        "<div class='modal-header '>" +
                            "<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>&times;</button>" +
                            "<h4 class='modal-title'>" +
                                title +
                            "</h4>" +
                        "</div>" +
                        "<div class='modal-body'>" +
                            message +
                        "</div>" +
                        "<div class='modal-footer'>" +
                            buttons +
                        "</div>" +
                    "</div>" +
                "</div>" +
            "</div>");

        newDialog.modal({ show: true });
    };

    //If an error code is returned from an ajax request, this function maps the code to a friendly error message,
    //displaying it to the user.
    var ajaxError = function (request, status, error)
                    {
        
                        var errorMessage = statusErrorMap[request.status];
                        if (!errorMessage)
                            errorMessage = "An error occured, the requested action could not be completed.";

                        var newDialog = $("<div class='modal fade'>" +
                                "<div class='modal-dialog'>" +
                                    "<div class='modal-content'>" +
                                        "<div class='modal-header '>" +
                                            "<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>&times;</button>" +
                                            "<h4 class='modal-title'>Error</h4>" +
                                        "</div>" +
                                        "<div class='modal-body'>" +
                                            errorMessage +
                                        "</div>" +
                                        "<div class='modal-footer'>" +
                                            "<button type='button' class='btn btn-default' data-dismiss='modal'>Close</button>" +
                                        "</div>" +
                                    "</div>" +
                                "</div>" +
                            "</div>");

                        newDialog.modal({ show: true });
                    };

    //Executes an Ajax query.
    var doAjaxQuery = function (url, successFunction, data, dataType) {
        var timeout = null;
        if (!disableAjaxModal) {
            timeout = setTimeout(function () { ajaxBlock(); }, 500);
            $(document).ajaxStop(ajaxUnblock);
        }
        
        $.ajax({
            type: "POST",
            url: url,
            data: data,
            dataType: dataType == null ? "json" : dataType,// "html",
            success: successFunction,
            error: function (request, status, error) {
                if (timeout != null) {
                    clearTimeout(timeout);
                }
                ajaxError(request, status, error);

            },
            complete: function () {
                if (timeout != null) {
                    clearTimeout(timeout);
                }
            }
        });
    };

    //Displays the passed message as a modal popup.
    var showError = function (message, title)
                    {

                        if (!message)
                            message = "An error occured, the requested action could not be completed.";

                        var newDialog = $("<div class='modal fade'>" +
                                "<div class='modal-dialog'>" +
                                    "<div class='modal-content'>" +
                                        "<div class='modal-header '>" +
                                            "<button type='button' class='close' data-dismiss='modal' aria-hidden='true'>&times;</button>" +
                                            "<h4 class='modal-title'>" +
                                                title +
                                            "</h4>" +
                                        "</div>" +
                                        "<div class='modal-body'>" +
                                            message +
                                        "</div>" +
                                        "<div class='modal-footer'>" +
                                            "<button type='button' class='btn btn-default' data-dismiss='modal'>Close</button>" +
                                        "</div>" +
                                    "</div>" +
                                "</div>" +
                            "</div>");

                        newDialog.modal({ show: true });
                    };

    return {
        DoQuery: doAjaxQuery,
        DisplayError: showError,
       /* Modal: buildModal,
        ModalCallback: buildModalCallback,*/
        MessageModal: buildMessageModal,
        EncodeHtml: encodeHtml,
        AjaxMessage: function (message) { ajaxMessage = message; },
        CustomAjaxModal: function (modalCode) { ajaxModal = modalCode; },
        DisableAjaxModal: function (disableModal) { disableAjaxModal = disableModal;}
    };
})();