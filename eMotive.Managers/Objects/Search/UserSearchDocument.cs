using System;
using Extensions;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.Users;
using eMotive.Search.Interfaces;

namespace eMotive.Managers.Objects.Search
{
    public class UserSearchDocument : ISearchDocument
    {
        public UserSearchDocument(User _user)
        {
            User = _user;
            DatabaseID = _user.ID;
            Title = string.Format("{0} {1}", User.Forename, User.Surname);
            Description = String.Empty;
            Type = "User";
            UniqueID = string.Format("{0}_{1}", Type, _user.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public User User { get; private set; }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(User.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            if (!string.IsNullOrEmpty(User.Username))
            {
                field = new Field("Username", User.Username, Field.Store.YES, Field.Index.ANALYZED);
                doc.Add(field);
            }
            if (!string.IsNullOrEmpty(User.Forename))
            {
                field = new Field("Forename", User.Forename, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }
            if (!string.IsNullOrEmpty(User.Surname))
            {
                field = new Field("Surname", User.Surname, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(User.Email))
            {
                field = new Field("Email", User.Email, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }

            if (User.Roles.HasContent())
            {
                foreach (var role in User.Roles)
                {
                    field = new Field("Role", role.Name, Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(field);
                    field = new Field("RoleID", role.ID.ToString(), Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(field);
                }
            }

            field = new Field("Enabled", User.Enabled.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Archived", User.Archived.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            return doc;
        }
    }
}
