using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dapper;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.Pages;

namespace eMotive.Repository.Objects
{
    /// <summary>
    /// CRUD operations on Pages and PartialPages.
    /// Pages are full pages with title and body.
    /// PartialPages are for sections of text on pages which contain dynamic content.
    /// </summary>
    public class MySqlPageRepository : IPageRepository
    {
        private readonly string connectionString;
        private readonly string pageUserFields;
        private readonly string partialUserFields;

        public MySqlPageRepository(string _connectionString)
        {
            connectionString = _connectionString;
            pageUserFields = "`id`, `title`, `body`, `created`, `updated`, `enabled`, `archived`";
            partialUserFields = "`id`, `key`, `text`, `description`";

        }
        public Page New()
        {
            return new Page();
        }

        public PartialPage NewPartial()
        {
            return new PartialPage();
        }

        public PartialPage Fetch(string _key)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `PartialPages` WHERE `key`=@key;", partialUserFields);

                var page = connection.Query<PartialPage>(sql, new { key = _key }).SingleOrDefault();

                return page;
            }
        }

        public IEnumerable<PartialPage> FetchPartial(IEnumerable<int> _ids)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `partialPages` WHERE `id` IN @ids;", partialUserFields);

                var pages = connection.Query<PartialPage>(sql, new { ids = _ids });

                return pages;
            }
        }

        public IEnumerable<PartialPage> FetchPartial(IEnumerable<string> _keys)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `partialPages` WHERE `key` IN @keys;", partialUserFields);

                var pages = connection.Query<PartialPage>(sql, new { keys = _keys });

                return pages;
            }
        }

        public IEnumerable<PartialPage> FetchAllPartial()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `PartialPages`;", partialUserFields);

                var newsItems = connection.Query<PartialPage>(sql);

                return newsItems;
            }
        }

        public bool Create(PartialPage _page, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();
                    const string sql = "INSERT INTO `PartialPages` (`key`, `text`, `description`) VALUES (@key, @text, @description);";

                    var success = connection.Execute(sql,
                        new
                        {
                            key = _page.Key,
                            text = _page.Text,
                            description = _page.Description
                        }) > 0;

                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);


                    transactionScope.Complete();

                    return success & id > 0;
                }
            }
        }

        public bool Update(PartialPage _page)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `PartialPages` SET `key`=@key, `text`=@text, `description`=@description WHERE `id`= @id;";

                var success = connection.Execute(sql, new
                {
                    key = _page.Key,
                    text = _page.Text,
                    description = _page.Description,
                    id = _page.ID
                });

                return success > 0;
            }
        }

        public bool Delete(PartialPage _page)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "DELETE FROM `PartialPages`WHERE `id`= @id;";

                var success = connection.Execute(sql, new { id = _page.ID });

                return success > 0;
            }
        }

        public Page Fetch(int _id, bool _enabled)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `Pages` WHERE `id`=@id AND `archived`=false{1};", pageUserFields, !_enabled ? string.Empty : " AND `enabled` = true");

                var page = connection.Query<Page>(sql, new { id = _id }).SingleOrDefault();

                return page;
            }
        }

        public IEnumerable<Page> FetchAll()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `Pages` WHERE `archived`=false;", pageUserFields);

                var newsItems = connection.Query<Page>(sql);

                return newsItems;
            }
        }

        public IEnumerable<Page> Fetch(IEnumerable<int> _ids, bool _enabled)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `Pages` WHERE `id` IN @ids AND `archived`=false {1} ORDER BY `created` DESC", pageUserFields, !_enabled ? string.Empty : "AND `enabled` = true");

                var pages = connection.Query<Page>(sql, new { ids = _ids });

                return pages;
            }
        }

        public bool Create(Page _page, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();
                    const string sql = "INSERT INTO `Pages` (`title`, `body`, `created`, `updated`, `enabled`,`archived`) VALUES (@title, @body, @created, @updated, @enabled, @archived);";

                    var success = connection.Execute(sql,
                        new
                        {
                            title = _page.Title,
                            body = _page.Body,
                            created = DateTime.Now,
                            updated = DateTime.Now,
                            enabled = _page.Enabled,
                            archived = _page.Archived,
                        }) > 0;

                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);


                    transactionScope.Complete();

                    return success & id > 0;
                }
            }
        }

        public bool Update(Page _page)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `Pages` SET `title`=@title, `body`=@body, `created`=@created, `updated`=@updated, `enabled`=@enabled,`archived`=@archived WHERE `id`= @id;";

                var success = connection.Execute(sql, new
                {
                    title = _page.Title,
                    body = _page.Body,
                    created = _page.Created,
                    updated = DateTime.Now,
                    enabled = _page.Enabled,
                    archived = _page.Archived,
                    id = _page.ID
                });

                return success > 0;
            }
        }

        public bool Delete(Page _page)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `Pages` SET `archived`=@archived WHERE `id`= @id;";

                var success = connection.Execute(sql, new
                {
                    archived = false,
                    id = _page.ID
                });

                return success > 0;
            }
        }


    }
}
