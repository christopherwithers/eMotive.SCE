using System.Collections.Generic;

namespace eMotive.Search.Objects
{
    public class SearchResult : Search
    {
        public virtual IEnumerable<ResultItem> Items { get; set; }
    }
}
