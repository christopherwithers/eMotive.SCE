Object.size = function (obj)
{
    var size = 0, key;
    for (key in obj) {
        if (obj.hasOwnProperty(key)) size++;
    }
    return size;
};


var RoleList = (function () {

    var records = {};

    //record template
    var record = function () {
        var id = 0;
        var name = "";
        var colour = "";

        this.getID = function () { return id; };
        this.setID = function (_id) { id = _id; };

        this.getName = function () { return name; };
        this.setName = function (_name) { name = _name; };

        this.getColour = function () { return colour; };
        this.setColour = function (_colour) { colour = _colour; };

        this.generateHTML = function () {
            return "<tr id=\"" + this.getID() + "\">" +
                   "<td><span class=\"label label-info\" id=\"" + this.getID() + "_colour\" style=\"background-color:#" + this.getColour() + "\">" + this.getName() + "</span></td>" +
                   "<td><span id=\"" + this.getID() + "_name\">" + this.getName() + "</span></td>" +
                   "<td><a href=\"#\" class=\"btn\" onclick=\"RoleList.UpdateRecord('" + this.getID() + "'); return false;\">Edit</a></td>" +
                   "<td><a href=\"#\" class=\"btn\" onclick=\"RoleList.DeleteRecord('" + this.getID() + "'); return false;\">Delete</a></td></tr>";
        };
    };

    var initialise = function(callback) {
        Ajax.DoQuery(window.Routes.URL("FetchAllRoles"), function (data) {

            if (data.results != undefined) {
                $.each(data.results, function(k, v) {

                    var newRecord = new record;

                    newRecord.setID(v.ID);
                    newRecord.setName(v.Name);
                    newRecord.setColour(v.Colour);
                    addRecord(v.ID, newRecord);
                });
            }

            if(callback != null)
                callback();
        });
    };

    var createRecord = function () {
        $('#CreateError').hide();

        $("#CreateModal").modal({ show: true });
    };

    var addRecord = function (id, newRecord) {
        records[id] = newRecord;
    };

    var deleteRecord = function (field) {
        var recordToDelete = RoleList.Records[field];
        $("#DeleteID").val(recordToDelete.getID());
        $("#DeleteMessage").html("Are you sure you wish to delete the role '" + recordToDelete.getName() + "'?");
        $('#DeleteError').hide();
        $("#DeleteModal").modal({ show: true });
    };


    var clearRecords = function () {
        records = {};
    };

    var updateRecord = function (field) {
        var editRecord = RoleList.Records[field];

        $("#EditID").val(editRecord.getID());
        $("#EditName").val(editRecord.getName());
        $("#EditColour").val(editRecord.getColour());
        document.getElementById('EditColour').color.fromString("'" + editRecord.getColour() + "'");
        $('#EditError').hide();

        $("#EditModal").modal({ show: true });
    };

    var buildList = function () {

        if (Object.size(records) == 0)
            return "<div class='alert alert-info'>No roles could be found.</div>";
        
        var html = "<table class=\"table table-striped table-hover\" id=\"RoleTable\">";

        $.each(records, function (key, value) {
            html += value.generateHTML();
        });
        html += "</table>";

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
