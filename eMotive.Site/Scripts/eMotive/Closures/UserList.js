Object.size = function (obj)
{
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};

var UserList = (function ()
{
    var records = {};
    //requires a rolelist
    var RoleList;

    //record template
    var record = function ()
    {
        var id;
        var username = "";
        var forename = "";
        var surname = "";
        var email = "";
        var enabled;
        var archived;
        var roles = {};

        this.getID = function () { return id; };
        this.setID = function (_id) { id = _id; };

        this.getUsername = function () { return username; };
        this.setUsername = function (_username) { username = _username; };

        this.getForename = function () { return forename; };
        this.setForename = function (_forename) { forename = _forename; };

        this.getSurname = function () { return surname; };
        this.setSurname = function (_surname) { surname = _surname; };

        this.getEmail = function () { return email; };
        this.setEmail = function (_email) { email = _email; };

        this.getEnabled = function () { return enabled; };
        this.setEnabled = function (_enabled) { enabled = _enabled; };

        this.getArchived = function () { return archived; };
        this.setArchived = function (_archived) { archived = _archived; };

        this.getRoles = function () { return roles; };
        this.setRoles = function (_roles) { roles = _roles; };

        this.generateHTML = function () {
            var disabled = "";
            if (!this.getEnabled()) {
                disabled = "class = 'error'";
            }

            return "<tr " + disabled + " id=\"" + this.getID() + "\">" +
                "<td>" + this.getUsername() + "</td>" +
                "<td>" + this.getForename() + "</td>" +
                "<td>" + this.getSurname() + "</td>" +
                //"<td>" + this.getEmail() + "</td>" +
                "<td>" + this.getEnabled() + "</td>" +
                //"<td>" + this.getArchived() + "</td>" +
                "<td><button class=\"btn btn-default btn-xs\" onclick=\"UserList.UpdateRecord('" + this.getID() + "'); return false;\">Edit</button> " +
                "<button class=\"btn btn-danger btn-xs\" onclick=\"UserList.DeleteRecord('" + this.getID() + "'); return false;\">Delete</button></td>" +
                "</tr>";
        };
    };

    //Generate a table with n columns to display roles. Add a checkbox group so we can collect select values
    var generateRoleList = function (name)
    {
        var html = "<table><tr>";
        var i = 0;
        $.each(this.RoleList.Records, function (key, value)
        {
            //  if ($("input[type=checkbox][value=" + value.ID + "]").length <= 0)
            html += "<td><input type=\"checkbox\" name=\"" + name + "\" value=\"" + value.getID() + "\"> <span class=\"label label-info\" style=\"background-color:#" + value.getColour() + "\">" + value.getName() + "</span></td>";
            i++;
            if (i % 3 == 0)
                html += "</tr><tr>";
        });
        html += "</tr></table>";
        return html;
    };

    var createRecord = function ()
    {
        $('#CreateError').hide();
        $('#Roles').html(generateRoleList("SelectedRoles"));
        $("#CreateModal").modal({ show: true });
    };

    var initialise = function (callback)
    {

        Ajax.DoQuery(window.Routes.URL("FetchUsers"), function (data)
        {
            if (data.results != undefined) {
                $.each(data.results, function(k, v) {

                    var newRecord = new UserList.RecordTemplate();

                    newRecord.setID(v.ID);
                    newRecord.setUsername(v.Username);
                    newRecord.setForename(v.Forename);
                    newRecord.setSurname(v.Surname);
                    newRecord.setEmail(v.Email);
                    newRecord.setEnabled(v.Enabled);
                    newRecord.setArchived(v.Archived);
                    newRecord.setRoles(v.Roles);
                    UserList.AddRecord(v.ID, newRecord);
                });
            }

            if (callback != null)
                callback();
        });
    };

    var addRecord = function (id, newRecord)
    {
        records[id] = newRecord;
    };

    var deleteRecord = function (field)
    {
        var recordToDelete = UserList.Records[field];
        $("#DeleteID").val(recordToDelete.getID());
        $("#DeleteMessage").html("Are you sure you wish to delete the account of '" + recordToDelete.getUsername() + "'?");
        $('#DeleteError').hide();
        $("#DeleteModal").modal({ show: true });
    };


    var clearRecords = function ()
    {
        records = {};
    };

    var updateRecord = function (field)
    {
        var editRecord = UserList.Records[field];

        $("#EditID").val(editRecord.getID());
        $("#EditUsername").val(editRecord.getUsername());
        $("#EditForename").val(editRecord.getForename());
        $("#EditSurname").val(editRecord.getSurname());
        $("#EditEmail").val(editRecord.getEmail());
        $("#EditEnabled").attr('checked', editRecord.getEnabled());
        $("#EditRoles").html(generateRoleList("EditRoles"));

        $.each(editRecord.getRoles(), function (key, value)
        {
            $("input[type=checkbox][name=EditRoles][value=" + value.ID + "]").attr('checked', true);
        });

        $('#EditError').hide();

        $("#EditModal").modal({ show: true });
    };

    var buildList = function ()
    {
        if (Object.size(records) == 0)
            return "<div class='alert alert-info'>No users could be found.</div>";
        
        var html = "<table class=\"table table-bordered table-striped table-hover data-table\" id=\"UserTable\"><thead>";
        html += "<tr><th>Username</th><th>Forename</th><th>Surname</th><th>Enabled</th><th></th></tr></thead><tbody>";
        $.each(records, function (key, value)
        {
            html += value.generateHTML();
        });
        html += "</tbody></table>";

        return html;
    };

    return {
        Initialise: initialise,
        AddRecord: addRecord,
        ClearRecords: clearRecords,
        Records: records,
        RecordTemplate: record,
        GenerateHtml: buildList,
        UpdateRecord: updateRecord,
        DeleteRecord: deleteRecord,
        CreateRecord: createRecord
    };
})();