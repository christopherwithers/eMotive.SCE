using System.Collections.Generic;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.Pages
{
    public class PartialPageSearch : BasicSearch
    {
        public PartialPageSearch()
        {
            ItemType = "Pages";
        }
        public IEnumerable<PartialPage> Pages { get; set; }
    }
}
