using System;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using eMotive.Repository.Objects.Pages;
using eMotive.Search.Interfaces;

namespace eMotive.Managers.Objects.Search
{
    public class PartialPageSearchDocument : ISearchDocument
    {
        public PartialPageSearchDocument(PartialPage _page)
        {
            PartialPage = _page;
            DatabaseID = _page.ID;
            Description = !string.IsNullOrEmpty(_page.Text) ? RemoveHtmlTagsFromString(_page.Text.Substring(0, _page.Text.Length > 50 ? 50 : _page.Text.Length - 1)) : string.Empty;
            Type = "PartialPage";
            UniqueID = string.Format("{0}_{1}", Type, _page.ID);
        }

        public int DatabaseID { get; set; }
        public string UniqueID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }

        public PartialPage PartialPage { get; private set; }

        internal string RemoveHtmlTagsFromString(string _text)
        {
            return Regex.Replace(_text, "<[^>]*>", String.Empty);
        }

        public Document BuildRecord()
        {
            var doc = new Document();

            var numericField = new NumericField("DatabaseID", Field.Store.YES, false);
            numericField.SetIntValue(PartialPage.ID);
            doc.Add(numericField);

            var field = new Field("UniqueID", UniqueID, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Description", Description, Field.Store.YES, Field.Index.NOT_ANALYZED);
            doc.Add(field);

            field = new Field("Type", Type, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("Text", PartialPage.Text, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("PageDescription", PartialPage.Description, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            field = new Field("Key", PartialPage.Key, Field.Store.NO, Field.Index.ANALYZED, Field.TermVector.NO);
            doc.Add(field);

            return doc;
        }
    }
}
