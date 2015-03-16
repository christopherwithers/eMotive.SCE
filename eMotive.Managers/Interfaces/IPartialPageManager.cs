using System.Collections.Generic;
using eMotive.Models.Objects.Pages;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;

namespace eMotive.Managers.Interfaces
{
    public interface IPartialPageManager : ISearchable
    {
        PartialPage New();
        PartialPage Fetch(string _key);
        bool Create(PartialPage _page, out int _id);
        bool Update(PartialPage _page);
        bool Delete(PartialPage _page);

        IEnumerable<PartialPage> FetchPartials(string[] _keys);
        IEnumerable<PartialPage> FetchRecordsFromSearch(SearchResult _searchResult);
    }
}
