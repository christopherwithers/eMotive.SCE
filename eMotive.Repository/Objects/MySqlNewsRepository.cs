using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Dapper;
using MySql.Data.MySqlClient;
using eMotive.Repository.Interfaces;
using eMotive.Repository.Objects.News;

namespace eMotive.Repository.Objects
{
    public class MySqlNewsRepository : INewsRepository
    {        
        private readonly string connectionString;
        private readonly string userFields;

        public MySqlNewsRepository(string _connectionString)
        {
            connectionString = _connectionString;
            userFields = "`id`, `title`, `body`, `authorid`, `created`, `updated`, `image`, `enabled`, `archived`";
        }

        public NewsItem New()
        {
            return new NewsItem();
        }

        public NewsItem Fetch(int _id, bool _enabled)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = string.Format("SELECT {0} FROM `News` WHERE `id`=@id AND `archived`=false{1};", userFields, !_enabled ? string.Empty : " AND `enabled` = true");

                var newsItem = connection.Query<NewsItem>(sql, new { id = _id }).SingleOrDefault();

                return newsItem;
            }
        }

        public IEnumerable<NewsItem> FetchAll()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `News` WHERE `archived`=false;", userFields);

                var newsItems = connection.Query<NewsItem>(sql);

                return newsItems;
            }
        }

        public IEnumerable<NewsItem> Fetch(IEnumerable<int> _ids, bool _enabled)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var sql = string.Format("SELECT {0} FROM `News` WHERE `id` IN @ids AND `archived`=false {1} ORDER BY `created` DESC", userFields, !_enabled ? string.Empty : "AND `enabled` = true");

                var newsItems = connection.Query<NewsItem>(sql, new { ids = _ids });

                return newsItems;
            }
        }

        public bool Create(NewsItem _newsItem, out int _id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                using (var transactionScope = new TransactionScope())
                {
                    connection.Open();
                    const string sql = "INSERT INTO `News` (`title`, `body`, `authorid`, `created`, `updated`, `image`, `enabled`,`archived`) VALUES (@title, @body, @authorid, @created, @updated, @image, @enabled, @archived);";

                    var success = connection.Execute(sql,
                        new
                    {
                        title = _newsItem.Title,
                        body = _newsItem.Body,
                        authorid = _newsItem.AuthorID,
                        created = DateTime.Now,
                        updated = DateTime.Now,
                        image = _newsItem.Image,
                        enabled = _newsItem.Enabled,
                        archived = _newsItem.Archived,
                    }) > 0;

                    var id = connection.Query<ulong>("SELECT CAST(LAST_INSERT_ID() AS UNSIGNED INTEGER);").SingleOrDefault();
                    _id = Convert.ToInt32(id);
   

                    transactionScope.Complete();

                    return success & id > 0;
                }
            }
        }

        public bool Update(NewsItem _newsItem)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `News` SET `title`=@title, `body`=@body, `authorid`=@authorid, `created`=@created, `updated`=@updated, `image`=@image, `enabled`=@enabled,`archived`=@archived WHERE `id`= @id;";

                var success = connection.Execute(sql, new
                    {
                        title = _newsItem.Title,
                        body = _newsItem.Body,
                        authorid = _newsItem.AuthorID,
                        created = _newsItem.Created,
                        updated = DateTime.Now,
                        image = _newsItem.Image,
                        enabled = _newsItem.Enabled,
                        archived = _newsItem.Archived,
                        id = _newsItem.ID
                    });

                return success > 0;
            }
        }

        public bool Delete(NewsItem _newsItem)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                const string sql = "UPDATE `News` SET `archived`=@archived WHERE `id`= @id;";

                var success = connection.Execute(sql, new
                {
                    archived = false,
                    id = _newsItem.ID
                });

                return success > 0;
            }
        }
    }
}
