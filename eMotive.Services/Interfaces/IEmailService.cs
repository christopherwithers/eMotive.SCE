using System.Collections.Generic;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Objects;
using Email = eMotive.Models.Objects.Email.Email;

namespace eMotive.Services.Interfaces
{
    public interface IEmailService : ISearchable
    {
        bool SendMail(Objects.Email _email);
        bool SendMail(string _key, string _to, IDictionary<string, string> _replacements);

        bool SendMail(string _key, ICollection<string> _to, IDictionary<string, string> _replacements);

        bool SendMail(string _key, string _to, IDictionary<string, string> _replacements, byte[] binaryAttachment, string _name, string _extension);
        bool SendMail(int _key, string _to, IDictionary<string, string> _replacements);

        bool SendEmailLog(string _key, string _username);

        IEnumerable<EmailLog> FetchEmailLog(string _username);
        IEnumerable<Email> FetchAll();

        Email New();
        bool CreateMessage(Email _message, out int _id);
        Email FetchMessage(string _key);
        bool SaveMessage(Email _message);

        IEnumerable<Email> FetchRecordsFromSearch(SearchResult _searchResult);
    }
}
