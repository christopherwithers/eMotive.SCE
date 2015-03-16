using System;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.Users;
using eMotive.Search.Interfaces;

namespace eMotive.Managers.Objects.Search
{
    public class RoleSearchDocument : ISearchDocument
    {
        public RoleSearchDocument(Role _role)
        {
            Role = _role;
            DatabaseID = _role.ID;
            Title = _role.Name;
            Description = String.Empty;
            Type = "Role";
            UniqueID = string.Format("{0}_{1}", Type, _role.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public Role Role { get; private set; }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(Role.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Username", Role.Name, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            return doc;
        }
    }
}
