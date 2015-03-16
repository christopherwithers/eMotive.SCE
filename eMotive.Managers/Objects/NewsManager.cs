using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Extensions;
using Lucene.Net.Search;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Models.Objects.News;
using eMotive.Models.Objects.Search;
using eMotive.Repository.Interfaces;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using repNews = eMotive.Repository.Objects.News;
using emSearch = eMotive.Search.Objects.Search;

namespace eMotive.Managers.Objects
{
    public class NewsManager : INewsManager
    {
        private readonly INewsRepository newsRep;
        private readonly ISearchManager searchManager;
        private readonly IUserManager userManager;

        public NewsManager(INewsRepository _newsRep, ISearchManager _searchManager, IUserManager _userManager)
        {
            newsRep = _newsRep;
            searchManager = _searchManager;
            userManager = _userManager;

            AutoMapperManagerConfiguration.Configure();
        }

        public INotificationService notificationService { get; set; }

        public NewsItem New()
        {
            return Mapper.Map<repNews.NewsItem, NewsItem>(newsRep.New());
        }

        public NewsItem Fetch(int _id)
        {
            var repItem = newsRep.Fetch(_id, false);

            if (repItem == null)
            {
                notificationService.AddError("The requested news item could not be found.");
                return null;
            }

            var modItem = Mapper.Map<repNews.NewsItem, NewsItem>(repItem);

            modItem.Author = userManager.Fetch(repItem.AuthorID);


            return modItem;
        }

        public bool Create(NewsItem _newsItem, out int _id)
        {
            var username = _newsItem.Author.Username;

            if (string.IsNullOrEmpty(username))
            {
                notificationService.AddError("The news article could not be created.");
                notificationService.Log("NewsManager: _newsItem.Author.Username not found in Create function.");
                _id = -1;
                return false;
            }

            var user = userManager.Fetch(username);

            if (user == null)
            {
                notificationService.AddError("The news article could not be created.");
                notificationService.Log("NewsManager: user not found by UserManager in Create function.");
            }

            var repItem = Mapper.Map<NewsItem, repNews.NewsItem>(_newsItem);
            repItem.AuthorID = user.ID;
            int id;
            if (newsRep.Create(repItem, out id))
            {
                repItem.ID = _id = id;
                searchManager.Add(new NewsSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("The news article could not be created.");
            _id = -1;
            return false;
        }

        public bool Update(NewsItem _newsItem)
        {
            var repItem = Mapper.Map<NewsItem, repNews.NewsItem>(_newsItem);

            if (newsRep.Update(repItem))
            {
                searchManager.Update(new NewsSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to update the news article.");
            return false;
        }

        public bool Delete(NewsItem _newsItem)
        {
            var repItem = Mapper.Map<NewsItem, repNews.NewsItem>(_newsItem);

            if (newsRep.Delete(repItem))
            {
                searchManager.Update(new NewsSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to delete the news article.");
            return false;
        }

        public IEnumerable<NewsItem> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            if (_searchResult.Items.HasContent())
            {
                var repItems = newsRep.Fetch(_searchResult.Items.Select(n => n.ID).ToList(), false);
                if (repItems.HasContent())
                {
                    var modelItemsDict = Mapper.Map<IEnumerable<repNews.NewsItem>, IEnumerable<NewsItem>>(repItems).ToDictionary(k => k.ID, v => v);
                    var userIds = repItems.Select(n => n.AuthorID);
                    var userDict = userManager.Fetch(userIds).ToDictionary(k => k.ID, v => v);
                    foreach (var user in repItems)
                    {
                        modelItemsDict[user.ID].Author = userDict[user.AuthorID];
                    }

                    return modelItemsDict.Select(n => n.Value);
                }
            }

            return null;
        }

        public SearchResult DoSearch(BasicSearch _search)
        {
            var newSearch = Mapper.Map<BasicSearch, emSearch>(_search);
            if (string.IsNullOrEmpty(_search.Query))
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"Type", new emSearch.SearchTerm {Field = "NewsItem", Term = Occur.SHOULD}}
                };
            }
            else
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"NewsItemTitle", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Body", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"AuthorID", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Created", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Updated", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}}
                };
            }

            return searchManager.DoSearch(newSearch);
        }

        public void ReindexSearchRecords()
        {
            var records = newsRep.FetchAll();

            if (!records.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in records)
            {
                searchManager.Add(new NewsSearchDocument(item));
            }
        }
    }
}
