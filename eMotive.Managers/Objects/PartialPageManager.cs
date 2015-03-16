using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Extensions;
using Lucene.Net.Search;
using eMotive.Managers.Interfaces;
using eMotive.Managers.Objects.Search;
using eMotive.Models.Objects.Pages;
using eMotive.Models.Objects.Search;
using eMotive.Repository.Interfaces;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using repPages = eMotive.Repository.Objects.Pages;
using emSearch = eMotive.Search.Objects.Search;

namespace eMotive.Managers.Objects
{
    public class PartialPageManager : IPartialPageManager
    {
        private readonly ISearchManager searchManager;
        private readonly IPageRepository pageRepository;

        public PartialPageManager(IPageRepository _pageRepository, ISearchManager _searchManager)
        {
            searchManager = _searchManager;
            pageRepository = _pageRepository;

            AutoMapperManagerConfiguration.Configure();
        }

        public INotificationService notificationService { get; set; }

        public PartialPage New()
        {
            return Mapper.Map<repPages.PartialPage, PartialPage>(pageRepository.NewPartial());
        }

        public PartialPage Fetch(string _key)
        {
            return FetchInternal(_key, false);
        }

        /// <summary>
        /// Sometimes we want to check if a page exists without firing logging - such as page creation.
        /// If we record 'not found' and a page is created, the redirect will take them to an error page
        /// </summary>
        /// <param name="_key"></param>
        /// <param name="_suppressLogging"></param>
        /// <returns></returns>
        private  PartialPage FetchInternal(string _key, bool _suppressLogging)
        {
            var repItem = pageRepository.Fetch(_key);

            if (repItem == null && !_suppressLogging)
            {
                notificationService.AddError("The requested page could not be found.");
                notificationService.Log(string.Format("The partial page '{0}' could not be found.", _key));
                return null;
            }

            var modItem = Mapper.Map<repPages.PartialPage, PartialPage>(repItem);

            return modItem;
        }

        public bool Create(PartialPage _page, out int _id)
        {
            var repItem = Mapper.Map<PartialPage, repPages.PartialPage>(_page);

            var checkPage = FetchInternal(_page.Key, true);

            if (checkPage != null && (checkPage.Key.ToLowerInvariant() == _page.Key.ToLowerInvariant()))
            {
                notificationService.AddIssue(string.Format("A partial page with the key '{0}' already exists.", checkPage.Key));
                _id = -1;
                return false;
            }

            int id;
            if (pageRepository.Create(repItem, out id))
            {
                repItem.ID = _id = id;
                searchManager.Add(new PartialPageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("The page could not be created.");
            _id = -1;
            return false;
        }

        public bool Update(PartialPage _page)
        {
            var repItem = Mapper.Map<PartialPage, repPages.PartialPage>(_page);

            if (pageRepository.Update(repItem))
            {
                searchManager.Update(new PartialPageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to update the page.");
            return false;
        }

        public bool Delete(PartialPage _page)
        {
            var repItem = Mapper.Map<PartialPage, repPages.PartialPage>(_page);

            if (pageRepository.Delete(repItem))
            {
                searchManager.Update(new PartialPageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to delete the page.");
            return false;
        }

        public IEnumerable<PartialPage> FetchPartials(string[] _keys)
        {
            return Mapper.Map<IEnumerable<repPages.PartialPage>, IEnumerable<PartialPage>>(pageRepository.FetchPartial(_keys)); 
        }

        public IEnumerable<PartialPage> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            if (_searchResult.Items.HasContent())
            {
                var repItems = pageRepository.FetchPartial(_searchResult.Items.Select(n => n.ID).ToList());

                if (repItems.HasContent())
                {
                    return Mapper.Map<IEnumerable<repPages.PartialPage>, IEnumerable<PartialPage>>(repItems);

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
                    {"Type", new emSearch.SearchTerm {Field = "PartialPage", Term = Occur.SHOULD}}
                };
            }
            else
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"PageTitle", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Text", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"PageDescription", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Key", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}}
                };
            }

            return searchManager.DoSearch(newSearch);
        }

        public void ReindexSearchRecords()
        {
            var records = pageRepository.FetchAllPartial();

            if (!records.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in records)
            {
                searchManager.Add(new PartialPageSearchDocument(item));
            }
        }
    }
}
