using System.Collections.Generic;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.Pages
{
    public class PageSearch : BasicSearch
    {
        public PageSearch()
        {
            ItemType = "Pages";
        }
        public IEnumerable<Page> Pages { get; set; }
    }
}
