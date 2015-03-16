using eMotive.Services.Interfaces;
using eMotive.Services.Objects.Dictionary.TinyMCE;
using ServiceStack.ServiceHost;
using ServiceStack.ServiceInterface;

namespace eMotive.Api
{
    [Authenticate]
    [Route("/Dictionary/Spelling", "Post")]
    public class CheckSpellings : SearchRequest
    {
    }

    public class DictionaryServices : Service
    {
        private readonly IDictionaryService _dictionaryService;

        public DictionaryServices(IDictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
        }

        public object Post(CheckSpellings request)
        {
            return _dictionaryService.ProcessRequest(request);
        }
    }
}
