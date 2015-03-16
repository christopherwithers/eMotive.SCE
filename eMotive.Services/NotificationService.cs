using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Services.Interfaces;
using eMotive.Services.Objects;

#if DEBUG
[assembly: InternalsVisibleTo("eMotive.Tests")]
#endif
namespace eMotive.Services
{
    
    public class NotificationService : INotificationService
    {
        
        internal readonly ICollection<Message> Messages;

        private readonly string connectionString;
        private readonly bool doLogging;

        public NotificationService(string _connectionString, string _enableLogging)
        {
            connectionString = _connectionString;

            if (!bool.TryParse(_enableLogging, out doLogging))
                doLogging = false;

            Messages = new Collection<Message>();
        }

        public void Log(string _log)
        {
            if (doLogging && !string.IsNullOrEmpty(_log))
                Messages.Add(new Message { MessageType = MessageType.Log, Details = _log });
        }

        public void AddError(string _error)
        {
            if(doLogging && !string.IsNullOrEmpty(_error))
                Messages.Add(new Message {MessageType = MessageType.Error, Details = _error});
        }

        public void AddIssue(string _issue)
        {
            if (doLogging && !string.IsNullOrEmpty(_issue))
                Messages.Add(new Message { MessageType = MessageType.Issue, Details = _issue });
        }

        public IEnumerable<string> FetchErrors()
        {
            if (!doLogging) return null;

            var errorMessages = Messages.Where(n => n.MessageType == MessageType.Error).ToList();

            return errorMessages.HasContent() ? errorMessages.Select(n => n.Details) : null;
        }

        public IEnumerable<string> FetchIssues()
        {
            if (!doLogging) return null;

            var errorMessages = Messages.Where(n => n.MessageType == MessageType.Issue).ToList();

            return errorMessages.HasContent() ? errorMessages.Select(n => n.Details) : null;
        }

        public void CommitDatabaseLog()
        {
            if (!doLogging) return;

            var dbLoggableErrors = Messages.Where(n => n.MessageType == MessageType.Log).ToList();

            if (!dbLoggableErrors.HasContent()) return;

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();

              //  using (var transactionScope = new TransactionScope())
              //  {
                    var insertObj = dbLoggableErrors.Select(n => new {Occurred = DateTime.Now, Error = n.Details});
                    conn.Execute("INSERT INTO `Log` (`Occurred`, `Error`) VALUES (@Occurred, @Error);", insertObj);

               // }

            }
        }
    }
}
