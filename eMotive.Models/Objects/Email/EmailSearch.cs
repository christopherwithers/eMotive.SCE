using System.Collections.Generic;
using eMotive.Models.Objects.Search;

namespace eMotive.Models.Objects.Email
{
    public class EmailSearch : BasicSearch
    {
        public EmailSearch()
        {
            ItemType = "Emails";
        }
        public IEnumerable<Email> Emails { get; set; }
        public bool CanCreate { get; set; }
    }
}
