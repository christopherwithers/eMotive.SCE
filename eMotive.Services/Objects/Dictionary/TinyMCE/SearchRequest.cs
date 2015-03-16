using System.Collections.Generic;

namespace eMotive.Services.Objects.Dictionary.TinyMCE
{
    public class SearchRequest
    {
        public string ID { get; set; }
        public string Method { get; set; }
        public Params Params { get; set; }
    }


    public class Params
    {
        public string Lang { get; set; }
        public IEnumerable<string> Words { get; set; }
    }
}
