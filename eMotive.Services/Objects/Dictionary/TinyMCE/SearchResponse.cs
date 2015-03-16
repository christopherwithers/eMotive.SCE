using System.Collections.Generic;

namespace eMotive.Services.Objects.Dictionary.TinyMCE
{
    public class SearchResponse
    {
        public string id { get; set; }
        public Dictionary<string, List<string>> result { get; set; }
        public string error { get; set; }
    }
}
