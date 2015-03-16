using System.Collections.Generic;
using eMotive.Repository.Objects.News;

namespace eMotive.Repository.Interfaces
{
    public interface INewsRepository
    {
        NewsItem New();
        NewsItem Fetch(int _id, bool _enabled);

        IEnumerable<NewsItem> FetchAll();
        IEnumerable<NewsItem> Fetch(IEnumerable<int> _ids, bool _enabled);

        bool Create(NewsItem _newsItem, out int _id);
        bool Update(NewsItem _newsItem);
        bool Delete(NewsItem _newsItem);
    }
}
