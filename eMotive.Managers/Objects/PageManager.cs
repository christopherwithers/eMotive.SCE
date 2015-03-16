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
    public class PageManager : IPageManager
    {
        private readonly ISearchManager searchManager;
        private readonly IPageRepository pageRepository;

        public PageManager(IPageRepository _pageRepository, ISearchManager _searchManager)
        {
            searchManager = _searchManager;
            pageRepository = _pageRepository;

            AutoMapperManagerConfiguration.Configure();
        }

        public INotificationService notificationService { get; set; }

        public Page New()
        {
            return Mapper.Map<repPages.Page, Page>(pageRepository.New());
        }

        public Page Fetch(int _id)
        {
            var repItem = pageRepository.Fetch(_id, false);

            if (repItem == null)
            {
                notificationService.AddError("The requested page could not be found.");
                return null;
            }

            var modItem = Mapper.Map<repPages.Page, Page>(repItem);


            return modItem;
        }

        public bool Create(Page _page, out int _id)
        {
            var repItem = Mapper.Map<Page, repPages.Page>(_page);

            int id;
            if (pageRepository.Create(repItem, out id))
            {
                repItem.ID = _id = id;
                searchManager.Add(new PageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("The page could not be created.");
            _id = -1;
            return false;
        }

        public bool Update(Page _page)
        {
            var repItem = Mapper.Map<Page, repPages.Page>(_page);

            if (pageRepository.Update(repItem))
            {
                searchManager.Update(new PageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to update the page.");
            return false;
        }

        public bool Delete(Page _page)
        {
            var repItem = Mapper.Map<Page, repPages.Page>(_page);

            if (pageRepository.Delete(repItem))
            {
                searchManager.Update(new PageSearchDocument(repItem));
                return true;
            }

            notificationService.AddError("An error occurred while trying to delete the page.");
            return false;
        }

        public IEnumerable<Page> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            if (_searchResult.Items.HasContent())
            {
                var repItems = pageRepository.Fetch(_searchResult.Items.Select(n => n.ID).ToList(), false);
                if (repItems.HasContent())
                {
                    return Mapper.Map<IEnumerable<repPages.Page>, IEnumerable<Page>>(repItems);

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
                    {"Type", new emSearch.SearchTerm {Field = "Page", Term = Occur.SHOULD}}
                };
            }
            else
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"PageTitle", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Body", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Created", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Updated", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}}
                };
            }

            return searchManager.DoSearch(newSearch);
        }

        public void ReindexSearchRecords()
        {
            var records = pageRepository.FetchAll();

            if (!records.HasContent())
            {
                //todo: send an error message here
                return;
            }

            foreach (var item in records)
            {
                searchManager.Add(new PageSearchDocument(item));
            }
        }
    }
}
