using System.Collections.Generic;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.News
{
    public class NewsSearch : BasicSearch
    {
        public NewsSearch()
        {
            ItemType = "News Items";
        }
        public IEnumerable<NewsItem> NewsItems { get; set; }
    }
}
