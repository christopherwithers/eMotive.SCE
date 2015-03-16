using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web;
using eMotive.Models.Objects.Search;
using Extensions;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using eMotive.Search.Interfaces;
using Version = Lucene.Net.Util.Version;

namespace eMotive.Search.Objects
{
    public class SearchManager : ISearchManager
    {
        private readonly FSDirectory directory;
        private static IndexWriter writer;

        private readonly PerFieldAnalyzerWrapper analyzer;
        private IndexSearcher searcher;
        private readonly Version luceneVersion;


        public SearchManager(string _indexLocation)
        {
            if(string.IsNullOrEmpty(_indexLocation))
                throw new FileNotFoundException("The lucene index could not be found.");

            luceneVersion = Version.LUCENE_30;

            var resolvedServerLocation = HttpContext.Current.Server.MapPath(string.Format("~{0}", _indexLocation));
            directory = FSDirectory.Open(new DirectoryInfo(resolvedServerLocation));

            analyzer = new PerFieldAnalyzerWrapper(new StandardAnalyzer(luceneVersion));
            analyzer.AddAnalyzer("Role", new KeywordAnalyzer());

            try
            {
                writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            }
            catch (LockObtainFailedException loEx)
            {
             /*   IndexWriter.Unlock(directory);

                var l = directory.MakeLock(resolvedServerLocation);
                l.Obtain();
                l.Release();*/
                try
                {
                    directory.DeleteFile("write.lock");
                }
                catch(IOException ioEx)
                {
                    //todo: error log this??
                }
                

                writer = new IndexWriter(directory, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED);
            }

            

        }

        public SearchResult DoSearch(Search _search)
        {
            //todo: is this needed? i.e. dispose first

            searcher = new IndexSearcher(writer.GetReader());

            //TODO: need to tidy this up, perhaps only initialise parser if _search.Query
            var items = new Collection<ResultItem>();
            try//todo: do i need to make title DocumentTITLE AND UNALAYZED AGAIN - thenadd an analyzed title in? YESSSSSSSSSSS
            {
                TopDocs docs;
                QueryWrapperFilter wrapper = null;

                var bq = new BooleanQuery();
                var parser = new QueryParser(luceneVersion, string.Empty, analyzer);
                if (!string.IsNullOrEmpty(_search.Query) && !_search.CustomQuery.HasContent())
                {
                    var query = parser.Parse(_search.Query);
                    bq = new BooleanQuery
                        {
                            {
                                parser.Parse(string.Format("Title:{0}", query)), Occur.MUST
                            },
                            {
                                parser.Parse(string.Format("Description:{0}", query)), Occur.MUST
                            }
                        };
                }
                else
                {
                    if (_search.CustomQuery.HasContent())
                    {
                        bq = new BooleanQuery();
                        //TODO: need a way of passing in occur.must and occur.should
                        foreach (var query in _search.CustomQuery.Where(n => !string.IsNullOrEmpty(n.Value.Field)))
                        {
                            bq.Add(new BooleanClause(parser.Parse(string.Format("{0}:{1}", query.Key, query.Value.Field)), query.Value.Term));
                        }
                    }
                }

                if (_search.Filters.HasContent())
                {
                    var filterBq = new BooleanQuery();
                    foreach (var filter in _search.Filters)
                    {
                        filterBq.Add(new BooleanClause(parser.Parse(string.Format("{0}:{1}", filter.Key, filter.Value.Field)), filter.Value.Term));
                    }
                    wrapper = new QueryWrapperFilter(filterBq);
                }

                if (_search.Filters.HasContent() && string.IsNullOrEmpty(_search.Query) &&
                    !_search.CustomQuery.HasContent())
                {//we can't search with filter alone, do we'll add filters as a custom query and search on them.
                    bq = new BooleanQuery();
                    foreach (var filter in _search.Filters)
                    {
                        bq.Add(new BooleanClause(parser.Parse(string.Format("{0}:{1}", filter.Key, filter.Value.Field)), filter.Value.Term));
                    }
                }

                if (string.IsNullOrEmpty(_search.Query)  && !_search.CustomQuery.HasContent() && !_search.Filters.HasContent())
                    throw new ArgumentException("Neither Query, CustomQuery nor a filter has been defined.");

                Sort sort = null;//new Sort(new SortField("Forename", SortField.STRING, true));

                if (!string.IsNullOrEmpty(_search.SortBy))
                {
                    sort = new Sort(new SortField(_search.SortBy, SortField.STRING, _search.OrderBy != SortDirection.ASC)); 
                }

                docs = sort != null ? searcher.Search(bq, wrapper, 10000, sort) : searcher.Search(bq, wrapper, 10000);

                if (docs.ScoreDocs.Length > 0)
                {
                    _search.NumberOfResults = docs.ScoreDocs.Length;// -1;

                    var page = _search.CurrentPage - 1;

                    var first = page * _search.PageSize;
                    int last;
                    var numPages = (int)Math.Ceiling((decimal)docs.ScoreDocs.Length / _search.PageSize);

                    if (_search.NumberOfResults > first + _search.PageSize)
                    {
                        last = first + _search.PageSize;
                    }
                    else
                    {
                        //todo: need conditional on here for to check there are more than 1 result pages???
                        //need to work out page number, then equiv max page number from search results!
                        if (_search.CurrentPage > numPages)
                        {
                            _search.CurrentPage = numPages;
                            first = 0;
                        }

                        last = _search.NumberOfResults;
                    }

                    for (var i = first; i < last; i++)
                    {
                        var scoreDoc = docs.ScoreDocs[i];

                        var score = scoreDoc.Score;

                        var docId = scoreDoc.Doc;

                        var doc = searcher.Doc(docId);

                        items.Add(new ResultItem
                        {
                            ID = Convert.ToInt32(doc.Get("DatabaseID")),
                            Title = doc.Get("Title"),
                            Type = doc.Get("Type"),
                            Description = doc.Get("Description"),
                            Score = score
                        });
                    }
                }
            }
            catch (ParseException)
            {

                _search.Error = "The search query was malformed. For help with searching, please click the help link.";
            }
            catch
            {
                _search.Error = "An error occured. Please try again.";
            }

            searcher.Dispose();
            searcher = null;
          //  reader.Dispose();
            return new SearchResult
            {
                CurrentPage = _search.CurrentPage,
                Error = _search.Error,
                NumberOfResults = _search.NumberOfResults,
                PageSize = _search.PageSize,
                Query = _search.Query,
                Items = items
            };
        }


        public bool Add(ISearchDocument _document)
        {
            var success = true;
            try
            {
                var doc = _document.BuildRecord();
                writer.AddDocument(doc);
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Update(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.UpdateDocument(new Term("UniqueID", _document.UniqueID), _document.BuildRecord());
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
        }

        public bool Delete(ISearchDocument _document)
        {
            var success = true;
            try
            {
                writer.DeleteDocuments(new Term("UniqueID", _document.UniqueID));
                writer.Commit();
            }
            catch (AlreadyClosedException)
            {
                success = false;
            }
            catch (Exception)
            {
                success = false;
            }

            return success;
            
        }

        public void DeleteAll()
        {
            writer.DeleteAll();
            writer.Commit();
        }

        public int NumberOfDocuments()
        { 
            var reader = writer.GetReader();

            var numDocs = reader.NumDocs();

            reader.Dispose();

            return numDocs;
        }

        public void Dispose()
        {
            if (writer != null)
            {
                writer.Commit();

                writer.Dispose();
            }

            if (directory != null)
            {
                directory.Dispose();
            }
        }
    }
}
