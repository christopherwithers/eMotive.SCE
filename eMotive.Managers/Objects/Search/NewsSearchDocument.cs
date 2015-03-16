using System;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.News;
using eMotive.Search.Interfaces;
using System.Text.RegularExpressions;

namespace eMotive.Managers.Objects.Search
{
    public class NewsSearchDocument : ISearchDocument
    {
        public NewsSearchDocument(NewsItem _newsItem)
        {
            NewsItem = _newsItem;
            DatabaseID = _newsItem.ID;
            Title = _newsItem.Title;
            Description = RemoveHtmlTagsFromString(_newsItem.Body.Substring(0, _newsItem.Body.Length > 50 ? 50 : _newsItem.Body.Length -1));
            Type = "NewsItem";
            UniqueID = string.Format("{0}_{1}", Type, _newsItem.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public NewsItem NewsItem { get; private set; }


        internal string RemoveHtmlTagsFromString(string _text)
        {
            return Regex.Replace(_text, "<[^>]*>", String.Empty);
        }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(NewsItem.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Title", Title, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.YES, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("NewsItemTitle", Title, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Body", NewsItem.Body, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            numericField = new NumericField("AuthorID", Field.Store.YES, false);
            numericField.SetIntValue(NewsItem.AuthorID);
            doc.Add(numericField);

            var date = DateTools.DateToString(NewsItem.Created, DateTools.Resolution.DAY);
            field = new Field("Created", date, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            date = DateTools.DateToString(NewsItem.Updated, DateTools.Resolution.DAY);
            field = new Field("Updated", date, Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Enabled", NewsItem.Enabled.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            field = new Field("Archived", NewsItem.Archived.ToString(), Field.Store.NO, Field.Index.ANALYZED);
            doc.Add(field);

            return doc;
        }
    }
}
