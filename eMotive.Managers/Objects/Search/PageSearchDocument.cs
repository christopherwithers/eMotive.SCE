using System;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.Pages;
using eMotive.Search.Interfaces;
using System.Text.RegularExpressions;

namespace eMotive.Managers.Objects.Search
{
    public class PageSearchDocument : ISearchDocument
    {
        public PageSearchDocument(Page _page)
        {
            Page = _page;
            DatabaseID = _page.ID;
            Title = _page.Title;
            Description = RemoveHtmlTagsFromString(_page.Body.Substring(0, _page.Body.Length > 50 ? 50 : _page.Body.Length - 1));
            Type = "Page";
            UniqueID = string.Format("{0}_{1}", Type, _page.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public Page Page { get; private set; }


        internal string RemoveHtmlTagsFromString(string _text)
        {
            return Regex.Replace(_text, "<[^>]*>", String.Empty);
        }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(Page.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("PageTitle", Title, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Body", Page.Body, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            var date = DateTools.DateToString(Page.Created, DateTools.Resolution.DAY);
            field = new Field("Created", date, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            date = DateTools.DateToString(Page.Updated, DateTools.Resolution.DAY);
            field = new Field("Updated", date, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Enabled", Page.Enabled.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Archived", Page.Archived.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            return doc;
        }
    }
}
