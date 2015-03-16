using System.Collections.Generic;
using eMotive.Models.Objects.Pages;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;

namespace eMotive.Managers.Interfaces
{
    public interface IPageManager : ISearchable
    {
        Page New();
        Page Fetch(int _id);
        bool Create(Page _page, out int _id);
        bool Update(Page _page);
        bool Delete(Page _page);
        IEnumerable<Page> FetchRecordsFromSearch(SearchResult _searchResult);
    }
}
