using System.Collections.Generic;

namespace eMotive.Services.Interfaces
{
    /// <summary>
    /// A simple notification bus. Allows three types of messages to be raised. An error will redirect to an Error page and display the errors.
    /// An issue is information that can be returned to a user as a warning e.g. Username already taken etc.
    /// A Log message will write any output to a 'log' database table.
    /// </summary>
    public interface INotificationService
    {
        void Log(string _log);
        void AddError(string _error);
        void AddIssue(string _issue);

        IEnumerable<string> FetchErrors();
        IEnumerable<string> FetchIssues();

        /// <summary>
        /// Writes all error messages to the `log` database table.
        /// </summary>
        void CommitDatabaseLog();
    }
}
