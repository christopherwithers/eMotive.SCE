$(function () {
    RoleList.Initialise(function () {
        $("#RoleList").html(RoleList.GenerateHtml());
        $("#CreateRole").click(function () {
            RoleList.CreateRecord();
        });
    });
    
    $("#EditSave").click(function ()
    {

        var id = $("#EditID").val();
        var name = $("#EditName").val();
        var colour = $("#EditColour").val();

        Ajax.DoQuery(window.Routes.URL("UpdateRole"),
            function (data)
            {
                if (data.success) {

                    var editRecord = RoleList.Records[id];
                    editRecord.setName(name);
                    editRecord.setColour(colour);
                    RoleList.Records[id] = editRecord;

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
            { ID: id, Name: name, Colour: colour });
    });

    $("#CreateSave").click(function ()
    {
        var name = $("#CreateName").val();
        var colour = $("#CreateColour").val();

        Ajax.DoQuery(window.Routes.URL("CreateRole"),
            function (data)
            {
                if (data.success) {

                    var newRecord = new RoleList.RecordTemplate();
                    newRecord.setID(data.results);
                    newRecord.setName(name);
                    newRecord.setColour(colour);
                    RoleList.AddRecord(data.results, newRecord);

                    $("#RoleTable > tbody:last").append(newRecord.generateHTML());
                    $("#CreateModal").modal('toggle');
                    $("#CreateName").val("");
                    $("#CreateColour").val("FFFFFF");
                    document.getElementById('CreateColour').color.fromString("'FFFFFF'");
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
            { ID: 0, Name: name, Colour: colour });
    });

    $("#ConfirmDelete").click(function ()
    {

        var id = $("#DeleteID").val();

        var deleteRecord = RoleList.Records[id];

        var name = deleteRecord.getName();
        var colour = deleteRecord.getColour();

        Ajax.DoQuery(window.Routes.URL("DeleteRole"),
            function (data)
            {
                if (data.success) {

                    $("#" + id).remove();
                    $("#DeleteModal").modal('toggle');

                    for (var item in RoleList.Records) {
                        if (RoleList.Records.hasOwnProperty(item) && RoleList.Records[item] == id) {
                            delete RoleList.Records[item];
                        }
                    }



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
            { ID: id, Name: name, Colour: colour });
    });
});