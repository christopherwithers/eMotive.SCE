using System.Collections.Generic;
using eMotive.Models.Objects.News;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using modSearch = eMotive.Models.Objects.Search;

namespace eMotive.Managers.Interfaces
{
    public interface INewsManager : ISearchable
    {
        NewsItem New();
        NewsItem Fetch(int _id);
        bool Create(NewsItem _newsItem, out int _id);
        bool Update(NewsItem _newsItem);
        bool Delete(NewsItem _newsItem);
        IEnumerable<NewsItem> FetchRecordsFromSearch(SearchResult _searchResult);
    }
}
