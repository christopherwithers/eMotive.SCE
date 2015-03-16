$(function ()
{
    UserList.Initialise(function ()
    {
        $("#UserList").html(UserList.GenerateHtml());
        UserList.RoleList = RoleList;
        UserList.RoleList.Initialise();
        $("#CreateUser").click(function ()
        {
            UserList.CreateRecord();
        });
    });

    $("#EditSave").click(function ()
    {

        var id = $("#EditID").val();

        var editRecord = UserList.Records[id];

        var username = $("#EditUsername").val();
        var forename = $("#EditForename").val();
        var surname = $("#EditSurname").val();
        var email = $("#EditEmail").val();
        var enabled = $("#EditEnabled").is(":checked") == true;
        var roles = [];
        var archived = editRecord.getArchived();

        $('input[name="EditRoles"]:checked').each(function ()
        {
            var role = UserList.RoleList.Records[this.value];

            roles.push({ ID: role.getID(), Name: role.getName(), Colour: role.getColour() });
        });

        Ajax.DoQuery(window.Routes.URL("UpdateUser"),
            function (data)
            {
                if (data.success) {
                    //todo: when we plug in search functionality, we need to check if we should insert or not i.e. if count is < 10


                    editRecord.setUsername(username);
                    editRecord.setForename(forename);
                    editRecord.setSurname(surname);
                    editRecord.setEmail(email);
                    editRecord.setEnabled(enabled);
                    editRecord.setRoles(roles);
                    UserList.AddRecord(data.results, editRecord);

                    $("#" + id).replaceWith(editRecord.generateHTML());
                    $("#EditModal").modal('toggle');


                } else {
                    var html = "";
                    $.each(data.message, function (key, value)
                    {
                        html += "<p>" + value + "</p>";
                    });
                    $("#EditErrorMessage").html(html);
                    $("#EditError").show();
                }
            },
            { ID: id, Username: username, Forename: forename, Surname: surname, Email: email, Enabled: enabled, Archived: archived, RoleString: JSON.stringify(roles) });
    });


    $("#CreateSave").click(function ()
    {
        var username = $("#CreateUsername").val();
        var forename = $("#CreateForename").val();
        var surname = $("#CreateSurname").val();
        var email = $("#CreateEmail").val();
        var enabled = $("#CreateEnabled").is(":checked") == true;
        var roles = [];

        $('input[name="SelectedRoles"]:checked').each(function ()
        {
            var role = UserList.RoleList.Records[this.value];

            roles.push({ ID: role.getID(), Name: role.getName(), Colour: role.getColour() });
        });
        //  jQuery.ajaxSettings.traditional = true;

        Ajax.DoQuery(window.Routes.URL("CreateUser"),
            function (data)
            {
                if (data.success) {
                    //todo: when we plug in search functionality, we need to check if we should insert or not i.e. if count is < 10
                    var newRecord = new UserList.RecordTemplate();
                    newRecord.setID(data.results);
                    newRecord.setUsername(username);
                    newRecord.setForename(forename);
                    newRecord.setSurname(surname);
                    newRecord.setEmail(email);
                    newRecord.setEnabled(enabled);
                    newRecord.setRoles(roles);
                    UserList.AddRecord(data.results, newRecord);

                    $("#UserTable > tbody:last").append(newRecord.generateHTML());

                    $("#CreateModal").modal('toggle');

                    $("#CreateUsername").val("");
                    $("#CreateForename").val("");
                    $("#CreateSurname").val("");
                    $("#CreateEmail").val("");
                    $("#CreateEnabled").attr('checked', false);


                } else {
                    var html = "";
                    $.each(data.message, function (key, value)
                    {
                        html += "<p>" + value + "</p>";
                    });
                    $("#CreateErrorMessage").html(html);
                    $("#CreateError").show();
                }
            },
            { ID: 0, Username: username, Forename: forename, Surname: surname, Email: email, Enabled: enabled, Archived: false, RoleString: JSON.stringify(roles) });
    });

    $("#ConfirmDelete").click(function ()
    {

        var id = $("#DeleteID").val();

        var deleteRecord = UserList.Records[id];

        var username = deleteRecord.getUsername();
        var forename = deleteRecord.getForename();
        var surname = deleteRecord.getSurname();
        var email = deleteRecord.getEmail();
        var enabled = deleteRecord.getEnabled();
        var roles = deleteRecord.getRoles();
        var archived = deleteRecord.getArchived();

        Ajax.DoQuery(window.Routes.URL("DeleteUser"),
            function (data)
            {
                if (data.success) {

                    $("#" + id).remove();
                    $("#DeleteModal").modal('toggle');

                } else {
                    var html = "";
                    $.each(data.message, function (key, value)
                    {
                        html += "<p>" + value + "</p>";
                    });
                    $("#DeleteErrorMessage").html(html);
                    $("#DeleteError").show();
                }
            },
            { ID: id, Username: username, Forename: forename, Surname: surname, Email: email, Enabled: enabled, Archived: archived, RoleString: JSON.stringify(roles) });
    });
});