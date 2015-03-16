using System;
using System.Globalization;
using Extensions;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.Users;
using eMotive.Search.Interfaces;

namespace eMotive.Managers.Objects.Search
{
    public class SCESearchDocument : ISearchDocument
    {
        public SCESearchDocument(SCEData _user)
        {
            User = _user;
            DatabaseID = _user.IdUser;
            Title = string.Format("{0} {1}", User.Forename, User.Surname);
            Description = String.Empty;
            Type = "User";
            UniqueID = string.Format("{0}_{1}", Type, _user.IdUser);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public SCEData User { get; private set; }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(DatabaseID);
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
            //  if (User.Roles.HasContent())
           // {
             //   foreach (var role in User.Roles)
             //   {
                    field = new Field("Role","SCE", Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(field);
                    field = new Field("RoleID", "6", Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(field);
              //  }
          //  }

            field = new Field("Enabled", User.Enabled.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Archived", User.Archived.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            if (!string.IsNullOrEmpty(User.Trust))
            {
                field = new Field("Trust", User.Trust, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }

            if (!string.IsNullOrEmpty(User.Grade))
            {
                field = new Field("Grade", User.Grade, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }

            //if (!string.IsNullOrEmpty(User.MainSpecialty))
           // {
                field = new Field("MainSpecialty", User.MainSpecialty.ToString(CultureInfo.InvariantCulture), Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
           // }

            field = new Field("Trained", User.Trained.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            if (!string.IsNullOrEmpty(User.GMCNumber))
            {
                field = new Field("GMCNumber", User.GMCNumber, Field.Store.NO, Field.Index.ANALYZED);
                doc.Add(field);
            }

            return doc;
        }
    }
}
