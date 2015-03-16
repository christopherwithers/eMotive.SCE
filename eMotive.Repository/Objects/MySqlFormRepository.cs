using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;
using Dapper;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Forms;
using Extensions;
using MySql.Data.MySqlClient;

namespace eMotive.Repository.Objects
{
    public class MySqlFormRepository : IFormRepository
    {        
        private readonly string _connectionString;
        private IDbConnection _connection;

        public MySqlFormRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        private IDbConnection Connection
        {
            get { return _connection ?? (_connection = new MySqlConnection(_connectionString)); }
        }

        public Form NewForm()
        {
            return new Form();
        }

        public bool CreateForm(Form form, out int id)
        {
            using (var cn = Connection)
            {
                using (var transaction = new TransactionScope())
                {
                    var success = true;
                    cn.Open();
                    id = -1;

                    var sql = "INSERT INTO `Forms` (`Name`) Values (@Name);";

                    success &= cn.Execute(sql, form) > 0;

                    var newId = cn.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    var convId = id = Convert.ToInt32(newId);

                    if (id > 0)
                    {
                        var formFields = form.Fields.Select(n => new
                        {
                            FormId = newId,
                            Field = n.Field,
                            Type = n.Type,
                            ListId = n.ListID,
                            Order = n.Order
                        }).ToList();


                    sql = "INSERT INTO `formItems` (`FormId`, `Field`, `Type`, `ListId`, `Order`) Values (@FormId, @Field, @Type, @ListId, @Order);";

                        success &= cn.Execute(sql, formFields) > 0;
                    }

                    transaction.Complete();

                    return success | newId > 0;
                }

            }
        }

        public bool UpdateForm(Form form)
        {
            using (var cn = Connection)
            {
                using (var transaction = new TransactionScope())
                {
                    cn.Open();
                    var success = true;
                    #region fetch old Form
                    var sql = "SELECT `Id`, `Name` FROM `Forms` WHERE `Id`=@id;";
                    var oldForm = cn.Query<Form>(sql, new { id = form.ID }).SingleOrDefault();

                    if (form != null)
                    {
                        sql = "SELECT `Id`,`FormId`, `Field`, `Type`,`ListId`,`Order` From `FormItems` WHERE `FormId`=@id ORDER BY `Order` ASC;";

                        oldForm.Fields = cn.Query<FormItem>(sql, new { id = form.ID });
                    }

                    #endregion

                    if (form.Name != oldForm.Name)
                    {
                        sql = "UPDATE `Forms` SET `Name`=@Name WHERE `ID`=@Id;";
                        success &= cn.Execute(sql, new {Name = form.Name, Id = form.ID}) > 0;
                    }

                    if (form.Fields.HasContent() && !oldForm.Fields.HasContent())
                    {//why??
                        sql = "DELETE FROM `FormItems` WHERE `FormId`=@Id;";
                        success &= cn.Execute(sql, new { Id = form.ID }) > 0;
                    }
                    else
                    {
                        if (form.Fields.HasContent() && !oldForm.Fields.HasContent())
                        {
                            var formFields = form.Fields.Select(n => new
                            {
                                FormId = form.ID,
                                Field = n.Field,
                                Type = n.Type,
                                ListId = n.ListID,
                                Order = n.Order
                            }).ToList();


                            sql = "INSERT INTO `FormItems` (`FormId`, `Field`, `Type`, `ListId`, `Order`) Values (@FormId, @Field, @Type, @ListId, @Order);";

                            success &= cn.Execute(sql, formFields) > 0;
                        }
                        else if (form.Fields.HasContent() && oldForm.Fields.HasContent())
                        {
                            var toDelete = oldForm.Fields.Where(n => !form.Fields.Any(m => n.ID == m.ID && m.ID > 0));
                            var toUpdate = form.Fields.Where(n => oldForm.Fields.Any(m => n.ID == m.ID));//todo: need to check for greater than 0 here?
                            var toCreate = form.Fields.Where(n => n.ID == 0).Select(m => new
                            {
                                FormId = form.ID,
                                Field = m.Field,
                                Type = m.Type,
                                ListId = m.ListID,
                                Order = m.Order
                            });

                            if (toDelete.HasContent())
                            {
                                sql = "DELETE FROM `FormItems` WHERE `Id`=@id";
                                success &= cn.Execute(sql, toDelete) > 0;
                            }

                            if (toUpdate.HasContent())
                            {
                                sql = "UPDATE `FormItems` SET `Field`=@Field, `Type`=@Type, `ListId`=@ListId, `Order`=@Order WHERE `Id`=@id";
                                success &= cn.Execute(sql, toUpdate) > 0;
                            }

                            if (toCreate.HasContent())
                            {
                                sql = "INSERT INTO `FormItems` (`FormId`, `Field`, `Type`, `ListId`, `Order`) Values (@FormId, @Field, @Type, @ListId, @Order);";
                                success &= cn.Execute(sql, toCreate) > 0;
                            }
                        }
                    }

                    if (!success)
                        transaction.Dispose();
                    else
                        transaction.Complete();

                    return success;
                }
            }
        }

        public bool DeleteForm(Form form)
        {
            throw new NotImplementedException();
        }

        public Form FetchForm(int id)
        {
            using (var cn = _connection)
            {
                var sql = "SELECT `Id`, `Name` FROM `Forms` WHERE `Id`=@id;";
                var form = cn.Query<Form>(sql, new {id = id}).SingleOrDefault();

                if (form != null)
                {
                    sql = "SELECT `Id`,`FormId`, `Field`, `Type`,`ListId`,`Order` From `FormItems` WHERE `FormId`=@id ORDER BY `Order` ASC;";

                    form.Fields = cn.Query<FormItem>(sql, new {id = id});
                }

                return form;
            }
        }

        public Form FetchForm(string name)
        {
            using (var cn = Connection)
            {
                var sql = "SELECT `Id`, `Name` FROM `Forms` WHERE `Name`=@name;";
                var form = cn.Query<Form>(sql, new { name = name }).SingleOrDefault();

                if (form != null)
                {
                    sql = "SELECT `Id`, `FormId`, `Field`, `Type`,`ListId`,`Order` From `FormItems` WHERE `FormId`=@id ORDER BY `Order` ASC;";

                    form.Fields = cn.Query<FormItem>(sql, new { id = form.ID });
                }

                return form;
            }
        }

        public IEnumerable<Form> FetchForm()
        {
            using (var cn = Connection)
            {
                var sql = "SELECT `Id`, `Name` FROM `Forms`;";
                var forms = cn.Query<Form>(sql);

                if (forms.HasContent())
                {
                    sql = "SELECT `Id`, `FormId`, `Field`, `Type`,`ListId`,`Order` From `FormItems` ORDER BY `Order` ASC;";

                    var items = cn.Query<FormItem>(sql);

                    if (items.HasContent())
                    {
                        var itemDict = items.GroupBy(m => m.FormId).ToDictionary(k => k.Key, v => v.ToList());

                        foreach (var item in forms)
                        {
                            item.Fields = itemDict[item.ID];
                        }
                    }
                }

                return forms;
            }
        }

        public IEnumerable<Form> FetchForm(IEnumerable<int> ids)
        {
            using (var cn = Connection)
            {
                var sql = "SELECT `Id`, `Name` FROM `Forms` WHERE `Id` IN @ids;";
                var forms = cn.Query<Form>(sql, new {ids = ids});

                if (forms.HasContent())
                {
                    sql = "SELECT `Id`, `FormId`, `Field`, `Type`,`ListId`,`Order` From `FormItems` WHERE `FormId` IN @ids ORDER BY `Order` ASC;";

                    var items = cn.Query<FormItem>(sql, new {ids = ids});

                    if (items.HasContent())
                    {
                        var itemDict = items.GroupBy(m => m.FormId).ToDictionary(k => k.Key, v => v.ToList());

                        foreach (var item in forms)
                        {
                            item.Fields = itemDict[item.ID];
                        }
                    }
                }

                return forms;
            }
        }

        public IEnumerable<FormType> FetchFormTypes()
        {
            using (var cn = Connection)
            {
                const string sql = "SELECT `Id`, `Type` FROM `FormItemTypes`;";

                return cn.Query<FormType>(sql);
            }
        }

        public FormList NewFormList()
        {
            return new FormList {Collection = new List<FormListItem>()};
        }

        public bool CreateFormList(FormList formList, out int id)
        {
            using (var cn = Connection)
            {
                using (var transaction = new TransactionScope())
                {
                    var success = true;
                    cn.Open();
                    id = -1;

                    var sql = "INSERT INTO `FormLists` (`Name`) Values (@Name);";

                    success &= cn.Execute(sql, formList) > 0;

                    var newId = cn.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    var convId = id = Convert.ToInt32(newId);

                    if (id > 0)
                    {
                        var formListItems = formList.Collection.Select(n => new
                        {
                            FormListID = newId,
                            Value = n.Value,
                            Text = n.Text
                        }).ToList();


                        sql = "INSERT INTO `FormListItems` (`FormListID`, `Value`, `Text`) Values (@FormListID, @Value, @Text);";

                        success &= cn.Execute(sql, formListItems) > 0;
                    }

                    transaction.Complete();

                    return success | newId > 0;
                }

            }
        }
        //TODO: find a way of passing around connection and transaction scope for all internal queries? To do this, we need a way of wrapping using statements around all calls, rather than each call having their own using statements?
        public bool UpdateFormList(FormList formList)
        {
            using (var cn = Connection)
            {
                using (var transaction = new TransactionScope())
                {
                    cn.Open();
                    var success = true;
                    #region fetch old FormList
                    var sql = "SELECT `ID`,`Name` FROM `FormLists` WHERE `ID` = @id;";

                    var oldFormList = cn.Query<FormList>(sql, new { id = formList.ID }).SingleOrDefault();

                    if (oldFormList != null)
                    {
                        sql = "SELECT `ID`, `FormListID`, `Value`, `Text` FROM `FormListItems` WHERE `FormListID` = @id;";
                        oldFormList.Collection = cn.Query<FormListItem>(sql, new { id = oldFormList.ID });
                    }
                    #endregion

                    sql = "UPDATE `FormLists` SET `Name`=@Name WHERE `ID`=@Id;";
                    success &= cn.Execute(sql, new { Name = formList.Name, Id = formList.ID }) > 0;

                    if (formList.Collection.HasContent() && !oldFormList.Collection.HasContent())
                    {//why??
                        sql = "DELETE FROM `FormListItems` WHERE `FormListID`=@Id;";
                        success &= cn.Execute(sql, new { Id = formList.ID }) > 0;
                    }
                    else
                    {
                        if (formList.Collection.HasContent() && !oldFormList.Collection.HasContent())
                        {
                            var formListItems = formList.Collection.Select(n => new
                            {
                                FormListID = formList.ID,
                                Value = n.Value,
                                Text = n.Text
                            }).ToList();


                            sql = "INSERT INTO `FormListItems` (`FormListID`, `Value`, `Text`) Values (@ApplicationID, @Name);";

                            success &= cn.Execute(sql, formListItems) > 0;
                        }
                        else if(formList.Collection.HasContent() && oldFormList.Collection.HasContent())
                        {
                            var toDelete = oldFormList.Collection.Where(n => !formList.Collection.Any(m => n.ID == m.ID && m.ID > 0));
                            var toUpdate = formList.Collection.Where(n => oldFormList.Collection.Any(m => n.ID == m.ID));//todo: need to check for greater than 0 here?
                            var toCreate = formList.Collection.Where(n => n.ID == 0).Select(m => new {FormListID = formList.ID, Value = m.Value, Text = m.Text});

                            if (toDelete.HasContent())
                            {
                                sql = "DELETE FROM `FormListItems` WHERE `Id`=@id";
                                success &= cn.Execute(sql, toDelete) > 0;
                            }

                            if (toUpdate.HasContent())
                            {
                                sql = "UPDATE `FormListItems` SET `Value`=@Value, `Text`=@Text WHERE `Id`=@id";
                                success &= cn.Execute(sql, toUpdate) > 0;
                            }

                            if (toCreate.HasContent())
                            {
                                sql = "INSERT INTO `FormListItems` (`FormListID`,`VALUE`,`TEXT`) VALUES (@FormListID,@Value,@Text);";
                                success &= cn.Execute(sql, toCreate) > 0;
                            }
                        }
                    }

                    if (!success)
                        transaction.Dispose();
                    else
                        transaction.Complete();

                    return success;
                }
            }
        }

        public bool DeleteFormList(FormList formList)
        {
            using (var cn = Connection)
            {
                using (var transaction = new TransactionScope())
                {
                    cn.Open();
                    var success = true;

                    var sql = "DELETE FROM `FormLists` WHERE `Id`=@id;";
                    success &= cn.Execute(sql, new {id = formList.ID}) > 0;

                    sql = "DELETE FROM `FormListItems` WHERE `FormListID`=@id;";
                    success &= cn.Execute(sql, new {id = formList.ID}) > 0;

                    if (!success)
                        transaction.Dispose();
                    else
                        transaction.Complete();

                    return success;
                }
            }
        }

        public FormList FetchFormList(int id)
        {
            using (var cn = Connection)
            {
                cn.Open();

                var sql = "SELECT `ID`,`Name` FROM `FormLists` WHERE `ID` = @id;";

                var collection = cn.Query<FormList>(sql, new { id = id }).SingleOrDefault();

                if (collection != null)
                {
                    sql = "SELECT `ID`, `FormListID`, `Value`, `Text` FROM `FormListItems` WHERE `FormListID` = @id;";
                    collection.Collection = cn.Query<FormListItem>(sql, new { id = collection.ID });
                }

                return collection;
            }
        }

        public FormList FetchFormList(string name)
        {
            using (var cn = Connection)
            {
                cn.Open();

                var sql = "SELECT `ID`,`Name` FROM `FormLists` WHERE `Name` = @name;";

                var collection = cn.Query<FormList>(sql, new { Name = name }).SingleOrDefault();

                if (collection != null)
                {
                    sql = "SELECT `ID`, `FormListID`, `Value`, `Text` FROM `FormListItems` WHERE `FormListID` = @id;";
                    collection.Collection = cn.Query<FormListItem>(sql, new { id = collection.ID });
                }

                return collection;
            }
        }

        public IEnumerable<FormList> FetchFormList()
        {
            using (var cn = Connection)
            {
                cn.Open();

                var sql = "SELECT `ID`,`Name` FROM `FormLists`;";

                var collection = cn.Query<FormList>(sql);

                if (collection.HasContent())
                {
                    sql = "SELECT `ID`, `FormListID`, `Value`, `Text` FROM `FormListItems`;";
                    var items = cn.Query<FormListItem>(sql);

                    if (items.HasContent())
                    {
                        var itemDict = items.GroupBy(m => m.FormListID).ToDictionary(k => k.Key, v => v.ToList());

                        foreach (var item in collection)
                        {
                            item.Collection = itemDict[item.ID];
                        }
                    }
                }

                return collection;
            }
        }

        public IEnumerable<FormList> FetchFormList(IEnumerable<int> ids)
        {
            using (var cn = Connection)
            {
                cn.Open();

                var sql = "SELECT `ID`,`Name` FROM `FormLists`  WHERE `ID` in @ids;";

                var collection = cn.Query<FormList>(sql, new { ids = ids });

                if (collection.HasContent())
                {
                    sql = "SELECT `ID`, `FormListID`, `Value`, `Text` FROM `FormListItems` WHERE `FormListID` in @ids;";
                    var items = cn.Query<FormListItem>(sql, new { ids = ids });

                    if (items.HasContent())
                    {
                        var itemDict = items.GroupBy(m => m.FormListID).ToDictionary(k => k.Key, v => v.ToList());

                        foreach (var item in collection)
                        {
                            item.Collection = itemDict[item.ID];
                        }
                    }
                }

                return collection;
            }
        }
    }
}
