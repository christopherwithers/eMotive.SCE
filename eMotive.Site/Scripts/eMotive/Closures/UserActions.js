var UserActions = (function () {
    var fetchEmailLog = function (_username, _forename, _surname) {
        Ajax.DoQuery(window.Routes.URL("FetchEmailSentLog"),
            function (data) {
                var html;
                $.each(data.results, function (key, value) {
                    html += "<tr><td>" + value.EmailKey + "</td><td>" + moment(value.DateSent).format("dddd, MMMM Do YYYY, h:mm:ss a") + "</td></tr>";
                });
                $("#EmailLogMessage").text("Email log for " + _forename + " " + _surname + " (" + _username + ").");
                $("#EmailLogTable > tbody").empty();
                $("#EmailLogTable > tbody:last").append(html);
                $("#EmailLog").modal('toggle');
            },
            { username: _username });
    };
    var viewInterviewSessions = function (_username, _forename, _surname) {
        Ajax.DoQuery(window.Routes.URL("FetchApplicantSignups"),
            function (data) {
                if (data.success == "False") {
                    Ajax.DisplayError(data.message, "Error");
                } else {
                    if (data.results.HasSignedUp == false) {
                        $("#SessionlogInformation").text("<div class=\"alert alert-info\">" + _forename + " " + _surname + " (" + _username + ") has not signed up to any interview dates.</div>");
                    } else {
                        var signups = "<div class=\"alert alert-info\">Session information for " + _forename + " " + _surname + " (" + _username + ")</div>";
                        signups += "<table class=\"table table-bordered table-striped table-condensed\"><thead><tr><th>Type</th><th>Date</th><th>Slot</th><th></th></tr></thead><tbody>";
                        $.each(data.results.SignupDetails, function() {
                            //moment(this.SignUpDate).format("dddd, MMMM Do YYYY") + " at " + this.SignUpDetails);
                            signups += "<tr><td>" + this.SignupGroup.Name + "</td><td>" + this.SignupDescription + "</td><td>" + this.SignUpDetails + "</td><td><a class=\"btn btn-sm btn-info\" href=\"/Admin/Signups/SignupDetails/" + this.SignupID + "\">View</button></td></tr>";
                        });
                        //  $("#SessionlogInformation").text(moment(data.results.SignUpDate).format("dddd, MMMM Do YYYY") + " at " + data.results.SignUpDetails);
                        signups += "</tbody></table>";
                        $("#SessionlogInformation").html(signups);
                    }
                }
            }, { username: _username });

        $("#SessionLog").modal('toggle');
    };

    var resendAccountCreationEmail = function (_username) {
        Ajax.DoQuery(window.Routes.URL("ResendAccountCreationEmail"),
            function (data) {
                if (data.success == "False") {
                    Ajax.DisplayError("The account creation email was not sent", "Error");
                }
            }, { username: _username });
    };

    var sendAttendanceCertificate = function (username) {
        /*  Ajax.DoQuery(window.Routes.URL("SessionAttendanceCertificate"),
        function (data) {
            if (data.success == "False") {
                Ajax.DisplayError(data.message, "Error");
            } else {

            }
        }, { username: username });*/
        $('#TrainingCertificateError').hide();
        Ajax.DoQuery(window.Routes.URL("FetchSCETrainingInformation"),
            function (data) {
                if (data.success) {
                    var list = $("#TrainingDatelists");
                    $.each(data.results, function (i, item) {
                        list.append($("<option />").val(item.ID).text(item.Date));
                    });
                    $("#CertificateForUsername").val(username);
                    $("#TrainingCertificateModal").modal({ show: true });



                } else {
                    $('#TrainingCertificateError').html(data.message);
                }
            }, { username: username });
    };

    var deleteRecord = function (username) {
        $("#DeleteID").val(username);
        $("#DeleteMessage").html("Are you sure you wish to delete the account of '" + username + "'?");
        $('#DeleteError').hide();
        $("#DeleteModal").modal({ show: true });
    };

    var htmlEnDeCode = (function () {
        var charToEntityRegex,
            entityToCharRegex,
            charToEntity,
            entityToChar;

        function resetCharacterEntities() {
            charToEntity = {};
            entityToChar = {};
            // add the default set
            addCharacterEntities({
                '&amp;': '&',
                '&gt;': '>',
                '&lt;': '<',
                '&quot;': '"',
                '&#39;': "'"
            });
        }

        function addCharacterEntities(newEntities) {
            var charKeys = [],
                entityKeys = [],
                key, echar;
            for (key in newEntities) {
                echar = newEntities[key];
                entityToChar[key] = echar;
                charToEntity[echar] = key;
                charKeys.push(echar);
                entityKeys.push(key);
            }
            charToEntityRegex = new RegExp('(' + charKeys.join('|') + ')', 'g');
            entityToCharRegex = new RegExp('(' + entityKeys.join('|') + '|&#[0-9]{1,5};' + ')', 'g');
        }

        function htmlEncode(value) {
            var htmlEncodeReplaceFn = function (match, capture) {
                return charToEntity[capture];
            };

            return (!value) ? value : String(value).replace(charToEntityRegex, htmlEncodeReplaceFn);
        }

        function htmlDecode(value) {
            var htmlDecodeReplaceFn = function (match, capture) {
                return (capture in entityToChar) ? entityToChar[capture] : String.fromCharCode(parseInt(capture.substr(2), 10));
            };

            return (!value) ? value : String(value).replace(entityToCharRegex, htmlDecodeReplaceFn);
        }

        resetCharacterEntities();

        return {
            htmlEncode: htmlEncode,
            htmlDecode: htmlDecode
        };
    })();

    var fetchApplicantData = function (_username) {

        Ajax.DoQuery(window.Routes.URL("FetchApplicantData"),
            function (data) {
                if (data.success == "False") {
                    Ajax.DisplayError(data.message, "Error");
                } else {

                }
            }, { username: _username });
    };

    var fetchNotes = function (_username) {
        $('#NotesError').hide();
        $('#NotesText').html("");
        $("#NotesUsernameTitle").html("Notes for " + _username);
        $("#NotesUsername").val(_username);

        // Ajax.DoQuery(window.Routes.URL("FetchUserNotes"), function (data) { $('#NotesText').html(data.results); }, { username: _username });
        Ajax.DoQuery(window.Routes.URL("FetchUserNotes"), function (data) { tinyMCE.activeEditor.setContent(htmlEnDeCode.htmlDecode(data.results)); }, { username: _username });

        $("#NotesModal").modal({ show: true });
    };

    return {
        FetchEmailLog: fetchEmailLog,
        FetchSessionLog: viewInterviewSessions,
        ResendAccountCreationEmail: resendAccountCreationEmail,
        DeleteRecord: deleteRecord,
        FetchApplicantData: fetchApplicantData,
        FetchNotes: fetchNotes,
        SendAttendanceCertificate: sendAttendanceCertificate
    };
})();