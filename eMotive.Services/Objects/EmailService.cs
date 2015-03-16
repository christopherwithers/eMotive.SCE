using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Transactions;
using System.Web;
using AutoMapper;
using Dapper;
using Extensions;
using Lucene.Net.Search;
using MySql.Data.MySqlClient;
using eMotive.Models.Objects.Search;
using eMotive.Search.Interfaces;
using eMotive.Search.Objects;
using eMotive.Services.Interfaces;
using emSearch = eMotive.Search.Objects.Search;

namespace eMotive.Services.Objects
{
    public class EmailService : IEmailService
    {
        private readonly IeMotiveConfigurationService config;
        private readonly INotificationService notificationService;
        private readonly ISearchManager searchManager;
        private readonly IDocumentManagerService documentManagerService;
        private readonly string connectionString;
        private readonly bool emailEnabled;

        public EmailService(IeMotiveConfigurationService _config, INotificationService _notificationService, 
            IDocumentManagerService _documentManagerService, ISearchManager _searchManager, string _connectionString)
        {
            notificationService = _notificationService;
            config = _config;
            searchManager = _searchManager;
            documentManagerService = _documentManagerService;
            connectionString = _connectionString;
            emailEnabled = config.EmailsEnabled();

            AutoMapperServicesConfiguration.Configure();
        }

        public bool SendMail(Email _email)
        {
            var success = true;

            if (_email != null)
            {
                var mailfrom = config.EmailFromAddress();

                if (string.IsNullOrEmpty(mailfrom))
                {
                    notificationService.Log("SendMail: Error in sendMail function: No 'from' email address was specified.");
                    notificationService.AddError("Error in sendMail function: No 'from' email address was specified.");
                    return false;
                }

                _email.From = mailfrom;

                var client = new SmtpClient();

                if (!_email.To.HasContent() || !_email.From.HasContent())
                {
                    notificationService.Log("SendMail: Error in sendMail function: Receipient list was empty.");
                    notificationService.AddError("Error in sendMail function: Receipient list was empty.");
                    return false;
                }

                var message = new MailMessage();

                foreach (var to in _email.To)
                {
                    message.To.Add(to);
                }

                message.From = new MailAddress(_email.From);

                if (_email.CC.HasContent())
                {
                    foreach (var cc in _email.CC)
                    {
                        message.CC.Add(cc);
                    }
                }

                if (_email.BCC.HasContent())
                {
                    foreach (var bcc in _email.BCC)
                    {
                        message.Bcc.Add(bcc);
                    }
                }

                message.Subject = _email.Title;
                message.Body = _email.Message;
                message.Priority = _email.Priority;
                message.IsBodyHtml = _email.IsBodyHtml;

                if (_email.Attachments.HasContent())
                {
                    foreach (var attachment in _email.Attachments)
                    {
                        message.Attachments.Add(attachment);
                    }
                }
                
                if (emailEnabled)
                {
                    try
                    {
                        client.Send(message);
                    }
                    catch (Exception ex)
                    {
                        notificationService.Log(string.Format("SendMail: {0}", ex));
                        notificationService.AddIssue("An error occurred when trying to send the email notification. The email has not been sent.");
                        success = false;
                    }
                }
            }
            else
            {
                notificationService.Log("SendMail: Error in sendMail function: Email is null.");
                notificationService.AddError("Error in sendMail function: Email is null.");
                return false;
            }

            return success;
        }

        public bool SendMail(int _id, string _to, IDictionary<string, string> _replacements)
        {
            var emails = FetchEmail(_id);

            if (!emails.HasContent())
                return false;

            var success = true;
            foreach (var email in emails)
            {
                var sb = new StringBuilder(email.Message);

                if (_replacements.HasContent())
                {
                    foreach (var replacment in _replacements)
                    {
                        sb.Replace(replacment.Key, replacment.Value);
                    }
                }

                var newEmail = new Email();
                newEmail.To.Add(_to);
                newEmail.Title = email.Title;
                newEmail.Message = System.Net.WebUtility.HtmlDecode(sb.ToString());
                newEmail.IsBodyHtml = true;
                newEmail.From = config.EmailFromAddress();
               // newEmail.Attachments = FetchEmailAttachments(_id);

                //will flop to false and stay there even if future 'true' booleans are evaluated.
                success &= SendMail(newEmail);
            }

            return success;
        }

        public bool SendMail(string _key, string _to, IDictionary<string, string> _replacements)
        {
            return SendMail(_key, _to, _replacements, null, string.Empty, string.Empty);
        }

        public bool SendMail(string _key, ICollection<string> _to, IDictionary<string, string> _replacements)
        {
            var email = FetchEmail(_key);

            if (email == null)
                return false;

            var sb = new StringBuilder(email.Message);
            if (_replacements.HasContent())
            {
                foreach (var replacment in _replacements)
                {
                    sb.Replace(replacment.Key, replacment.Value);
                }
            }

            var newEmail = new Email();
            foreach (var address in _to)
            {
                newEmail.To.Add(address);
            }
            newEmail.Title = email.Title;
            newEmail.Message = System.Net.WebUtility.HtmlDecode(sb.ToString());
            newEmail.IsBodyHtml = true;
            newEmail.From = config.EmailFromAddress();
            newEmail.Attachments = FetchEmailAttachments(_key);

            return SendMail(newEmail);
        }

        public bool SendMail(string _key, string _to, IDictionary<string, string> _replacements, byte[] binaryAttachment, string _name, string _extension)
        {
            var email = FetchEmail(_key);

            if (email == null)
                return false;

            var sb = new StringBuilder(email.Message);
            if (_replacements.HasContent())
            {
                foreach (var replacment in _replacements)
                {
                    sb.Replace(replacment.Key, replacment.Value);
                }
            }

            var newEmail = new Email();
            newEmail.To.Add(_to);
            newEmail.Title = email.Title;
            newEmail.Message = System.Net.WebUtility.HtmlDecode(sb.ToString());
            newEmail.IsBodyHtml = true;
            newEmail.From = config.EmailFromAddress();
            newEmail.Attachments = FetchEmailAttachments(_key);


            if (binaryAttachment != null)
            {
                var mimeTypeDict = documentManagerService.FetchMimeTypeDictionary();
                MimeType type;
                if (!mimeTypeDict.TryGetValue(_extension, out type))
                {
                    type = documentManagerService.FetchDefaultMimeType();
                }

                var attachement = new Attachment(new MemoryStream(binaryAttachment), _name ?? string.Format("attachment.{0}", type.Extension), type.Type);

                if(!newEmail.Attachments.HasContent())
                    newEmail.Attachments = new Collection<Attachment>();

                newEmail.Attachments.Add(attachement);
            }

            return SendMail(newEmail);
        }

        public bool SendEmailLog(string _key, string _username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "INSERT INTO `emailSentLog` (`key`,`username`,`date`) VALUES (@key, @username, @date);";

                return connection.Execute(sql, new {key = _key, username = _username, date = DateTime.Now}) > 0;
            }
        }

        public IEnumerable<EmailLog> FetchEmailLog(string _username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT `key` as 'EmailKey',`date` as 'DateSent' FROM `emailSentLog` WHERE `username`=@username;";
                return  connection.Query<EmailLog>(sql, new { username = _username});
            }
        }

        public IEnumerable<Models.Objects.Email.Email> FetchAll()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = "SELECT `ID`,`key`, `title`, `message`, `description`, `Custom` FROM `emails`;";
                var emails = connection.Query<EditableEmail>(sql);

                return Mapper.Map<IEnumerable<EditableEmail>, IEnumerable<Models.Objects.Email.Email>>(emails);
            }
        }

        public SearchResult DoSearch(BasicSearch _search)
        {
            var newSearch = Mapper.Map<BasicSearch, emSearch>(_search);
            if (string.IsNullOrEmpty(_search.Query))
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"Type", new emSearch.SearchTerm {Field = "Email", Term = Occur.SHOULD}}
                };
            }
            else
            {
                newSearch.CustomQuery = new Dictionary<string, emSearch.SearchTerm>
                {
                    {"EmailTitle", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"EmailDescription", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Message", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}},
                    {"Key", new emSearch.SearchTerm {Field = _search.Query, Term = Occur.SHOULD}}
                };
            }

            return searchManager.DoSearch(newSearch);
        }

        public IEnumerable<Models.Objects.Email.Email> FetchRecordsFromSearch(SearchResult _searchResult)
        {
            if (_searchResult.Items.HasContent())
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    const string sql = "SELECT `ID`,`key`, `title`, `message`, `description`, `Custom` FROM `emails` WHERE `ID` IN @ids;";
                    var emails = connection.Query<EditableEmail>(sql, new {ids = _searchResult.Items.Select(n => n.ID)});

                    return Mapper.Map<IEnumerable<EditableEmail>, IEnumerable<Models.Objects.Email.Email>>(emails);
                }
            }
            return null;
        }

        public ICollection<Attachment> FetchEmailAttachments(string _key)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT a.* FROM `emailAttachments` a INNER JOIN `emails` b ON a.`emailId` = b.`Id` WHERE b.`key`=@key";

                var items = connection.Query<EmailAttachmentInfo>(sql, new { key = _key });

                if (!items.HasContent())
                    return null;

                var attachments = new Collection<Attachment>();

                var mimeTypeDict = documentManagerService.FetchMimeTypeDictionary();
                /*using (MemoryStream ms = new MemoryStream()) {
      using (FileStream file = new FileStream("file.bin", FileMode.Open, FileAccess.Read)) {
        byte[] bytes = new byte[file.Length];
        file.Read(bytes, 0, (int)file.Length);
        ms.Write(bytes, 0, (int)file.Length);
      }
    }*/
                foreach (var item in items)
                {
                    MimeType type;
                    if (!mimeTypeDict.TryGetValue(item.Extension, out type))
                    {
                        type = documentManagerService.FetchDefaultMimeType();
                    }

                    var filePath = HttpContext.Current.Server.MapPath(string.Format("~/{0}{1}{2}", item.Location, item.name, item.Extension));

                    var attachment = new Attachment(filePath) {ContentType = new System.Net.Mime.ContentType(type.Type)};
                    attachment.ContentDisposition.FileName = string.Format("{0}{1}", item.name, item.Extension);

                    attachments.Add(attachment);
                }

                return attachments;
            }
        }

        public Models.Objects.Email.Email New()
        {
            return Mapper.Map<EditableEmail, Models.Objects.Email.Email>(new EditableEmail());
        }

        public bool CreateMessage(Models.Objects.Email.Email _email, out int _id)
        {
            var email = Mapper.Map<Models.Objects.Email.Email, EditableEmail>(_email);

            var checkMessage = FetchEmail(email.Key);

            if (checkMessage != null && (checkMessage.Key.ToLowerInvariant() == email.Key.ToLowerInvariant()))
            {
                notificationService.AddIssue(string.Format("An email with the key '{0}' already exists.", email.Key));
                _id = -1;
                return false;
            }

            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();
                    const string sql = "INSERT INTO `emails` (`key`, `title`, `message`, `description`) VALUES (@key, @title, @message, @description);";
                    var success = connection.Execute(sql, email) > 0;

                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);
                    transactionScope.Complete();

                    if (success & id > 0)
                    {
                        email.ID = _id;
                        searchManager.Add(new EmailSearchDocument(email));
                        return true;
                    }

                    notificationService.AddIssue("An error occurred while trying to save the email.");
                    _id = -1;
                    return false;
                }
            }
        }

        internal EditableEmail FetchEmail(string _key)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT `ID`,`key`, `title`, `message`, `description`, `Custom` FROM `emails` WHERE `key`=@key;";
                return connection.Query<EditableEmail>(sql, new { key = _key }).SingleOrDefault();
            }
        }

        internal IEnumerable<EditableEmail> FetchEmail(int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT b.`ID`, b.`key`, b.`title`, b.`message`, b.`description`, b.`Custom` FROM `emailevents` a INNER JOIN `emails` b ON a.`idEmail` = b.`id` WHERE a.`id`=@_id;";
                return connection.Query<EditableEmail>(sql, new { id = _id });
            }
        }

        public Models.Objects.Email.Email FetchMessage(string _key)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT `ID`,`key`, `title`, `message`, `description`, `Custom` FROM `emails` WHERE `key`=@key;";
                var email = connection.Query<EditableEmail>(sql, new { key = _key }).SingleOrDefault();

                if (email == null)
                {
                    notificationService.AddError("The requested email message could not be found.");
                    return null;
                }
                return Mapper.Map<EditableEmail, Models.Objects.Email.Email>(email);
            }
        }

        public bool SaveMessage(Models.Objects.Email.Email _email)
        {
            var email = Mapper.Map<Models.Objects.Email.Email, EditableEmail>(_email);

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "UPDATE `emails` SET `title` =@title, `message`=@message, `description`=@description WHERE `key`=@key;";
                var success = connection.Execute(sql, email) > 0;
            
                if (!success)
                {
                    notificationService.Log(string.Format("EmailService: An error occured trying to update an email (key:{0})", email.Key));
                    notificationService.AddIssue("An error occurred while trying to save the email.");
                    return false;
                }

                searchManager.Update(new EmailSearchDocument(email));
                return true;
            }
        }

        public void ReindexSearchRecords()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                const string sql = "SELECT `ID`,`key`, `title`, `message`, `description` FROM `emails`;";
                var emails = connection.Query<EditableEmail>(sql);

                if (!emails.HasContent())
                {
                    //todo: send an error message here
                    return;
                }

                foreach (var item in emails)
                {
                    searchManager.Add(new EmailSearchDocument(item));
                }
            }
        }
    }
}
