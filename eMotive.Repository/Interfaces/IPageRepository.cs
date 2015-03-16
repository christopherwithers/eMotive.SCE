using System.Collections.Generic;
using eMotive.Repository.Objects.Pages;

namespace eMotive.Repository.Interfaces
{
    public interface IPageRepository
    {
        Page New();
        Page Fetch(int _id, bool _enabled);

        IEnumerable<Page> FetchAll();
        IEnumerable<Page> Fetch(IEnumerable<int> _ids, bool _enabled);

        bool Create(Page _page, out int _id);
        bool Update(Page _page);
        bool Delete(Page _page);

        PartialPage NewPartial();
        PartialPage Fetch(string _key);
        IEnumerable<PartialPage> FetchPartial(IEnumerable<int> _ids);
        IEnumerable<PartialPage> FetchPartial(IEnumerable<string> _keys);

        IEnumerable<PartialPage> FetchAllPartial();

        bool Create(PartialPage _page, out int _id);
        bool Update(PartialPage _page);
        bool Delete(PartialPage _page);
    }
}
