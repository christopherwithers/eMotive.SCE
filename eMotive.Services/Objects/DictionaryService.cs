using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects.Dictionary.TinyMCE;
using NHunspell;

namespace eMotive.Services.Objects
{
    public class DictionaryService : IDictionaryService
    {
        private readonly Hunspell _hunspell;

        public DictionaryService(string dictionaryRoot)
        {
            _hunspell = new Hunspell(string.Format("{0}en-GB.aff", dictionaryRoot), string.Format("{0}en-GB.dic", dictionaryRoot));
        }

        public SearchResponse ProcessRequest(SearchRequest request)
        {
            var dictionary = new Dictionary<string, List<string>>();

            foreach (var word in request.Params.Words)
            {
                if (_hunspell.Spell(word)) continue;

                List<string> currenSuggestions;
                if (!dictionary.TryGetValue(word, out currenSuggestions))
                    dictionary.Add(word, new List<string>());

                dictionary[word].AddRange(_hunspell.Suggest(word));
            }
            return new SearchResponse { id = request.ID, error = string.Empty, result = dictionary };
        }

        public void Dispose()
        {
            if (!_hunspell.IsDisposed)
                _hunspell.Dispose();
        }
    }
}
