using System;
using System.Globalization;
using eMotive.Models.Objects.SignupsMod;
using Extensions;
using Lucene.Net.Documents;
using eMotive.Search.Interfaces;

namespace eMotive.Managers.Objects.Search
{
    public class SignupSearchDocument : ISearchDocument
    {
        public SignupSearchDocument(Signup _signup)
        {
            Signup = _signup;
            DatabaseID = _signup.Id;
            Title = _signup.Description;//string.Format("{0} {1}", User.Forename, User.Surname);
            Description = String.Empty;
            Type = "Signup";
            UniqueID = string.Format("{0}_{1}", Type, _signup.Id);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public Signup Signup { get; private set; }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(Signup.Id);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("SignupDescription", Signup.Description, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("SlotStartDate", Signup.Date.ToString("dddd d MMMM yyyy"), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("SlotCloseDate", Signup.CloseDate.ToString("dddd d MMMM yyyy"), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("GroupID", Signup.Group.ID.ToString(CultureInfo.InvariantCulture), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Group", Signup.Group.Name, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("SiteID", Signup.Location.ID.ToString(CultureInfo.InvariantCulture), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Site", Signup.Location.Name, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Closed", Signup.Closed.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            if (Signup.Slots.HasContent())
            {
                foreach (var slot in Signup.Slots)
                {
                    field = new Field("SlotDescription", slot.Description, Field.Store.NO, Field.Index.NOT_ANALYZED);
                    doc.Add(field);

                    //field = new Field("SlotDate", slot.ToString("dddd d MMMM yyyy"), Field.Store.NO, Field.Index.NOT_ANALYZED);
                 //   doc.Add(field);

                    field = new Field("SlotEnabled", slot.Enabled.ToString(), Field.Store.NO, Field.Index.ANALYZED);
                    doc.Add(field);

                    if (slot.UsersSignedUp.HasContent())
                    {
                        foreach (var user in slot.UsersSignedUp)
                        {
                            numericField = new NumericField("SlotUserSignedupID", Field.Store.YES, false);
                            numericField.SetIntValue(user.User.ID);
                            doc.Add(numericField);

                            field = new Field("Username", user.User.Username, Field.Store.NO, Field.Index.ANALYZED);
                            doc.Add(field);

                            field = new Field("Forename", user.User.Forename, Field.Store.NO, Field.Index.ANALYZED);
                            doc.Add(field);

                            field = new Field("Surname", user.User.Surname, Field.Store.NO, Field.Index.ANALYZED);
                            doc.Add(field);

                            field = new Field("Email", user.User.Email, Field.Store.NO, Field.Index.ANALYZED);
                            doc.Add(field);
                        }
                    }
                }
            }

            return doc;
        }
    }
}
