using eMotive.Models.Objects.Search;
using eMotive.Search.Objects;

namespace eMotive.Search.Interfaces
{
    public interface ISearchable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_search"></param>
        /// <returns></returns>
        SearchResult DoSearch(BasicSearch _search);
        /// <summary>
        /// To pull items from their datastore and reindex them in lucene.
        /// </summary>
        void ReindexSearchRecords();
    }
}
