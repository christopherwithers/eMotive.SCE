using Lucene.Net.Documents;
using eMotive.Search.Interfaces;

namespace eMotive.Services.Objects
{
    public class EmailSearchDocument : ISearchDocument
    {
        public EmailSearchDocument(EditableEmail _email)
        {
            EditableEmail = _email;
            DatabaseID = _email.ID;
            Title = _email.Title;
            Description = string.Format("Email message belonging to the key '{0}'.", _email.Key);
            Type = "Email";
            UniqueID = string.Format("{0}_{1}", Type, _email.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public EditableEmail EditableEmail { get; private set; }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(EditableEmail.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("EmailTitle", EditableEmail.Title, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("Message", EditableEmail.Message, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("EmailDescription", EditableEmail.Description, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("Key", EditableEmail.Key, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            return doc;
        }
    }
}
