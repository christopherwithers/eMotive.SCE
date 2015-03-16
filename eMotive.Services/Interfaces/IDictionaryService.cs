using System;
using eMotive.Services.Objects.Dictionary.TinyMCE;

namespace eMotive.Services.Interfaces
{
    public interface IDictionaryService : IDisposable
    {
        SearchResponse ProcessRequest(SearchRequest request);
    }
}
