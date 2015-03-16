using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Extensions;
using MySql.Data.MySqlClient;
using eMotive.Models.Objects.Uploads;
using eMotive.Services.Interfaces;

namespace eMotive.Services.Objects
{
    public class DocumentManagerService : IDocumentManagerService
    {
        private readonly string connectionString;
        
        public DocumentManagerService(string _connectionString)
        {
            connectionString = _connectionString;
        }

      //  [Inject]
        public INotificationService notificationService { get; set; }

        //todo: pull this from dict! perhaps cache dict too.
        public MimeType FetchMimeTypeForExtension(string _extension)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT `extension`, `type` FROM `mimetypes`;";

                var types = connection.Query<MimeType>(sql);

                if (!types.HasContent())
                {
                    var type = types.SingleOrDefault(n => n.Extension.ToLower() == _extension.ToLower());

                    if (type != null)
                        return type;
                }

                return FetchDefaultMimeType();
            }
        }

        public IDictionary<string, MimeType> FetchMimeTypeDictionary()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT `extension`, `type` FROM `mimetypes`;";

                return connection.Query<MimeType>(sql).ToDictionary(k => k.Extension, v => v);
            }
        }

        public MimeType FetchDefaultMimeType()
        {
            return new MimeType {ID = -1, Extension = string.Empty, Type = "application/octet-stream"};
        }

        public bool SaveDocumentInformation(UploadedDocument _document)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "INSERT INTO `FileUploads` (`name`,`location`,`extension`, `modifiedName`, `DateUploaded`, `UploadedByUsername`, `Reference`) VALUES (@Name, @Location, @Extension, @ModifiedName, @DateUploaded, @UploadedByUsername, @Reference);";

                return connection.Execute(sql, _document) > 0;
            }
        }

        public IDictionary<UploadReference, UploadLocation> FetchUploadLocationDictionary()
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT `id`, `Reference`, `Directory` FROM `uploadLocations`;";

                var locationDictionary = connection.Query<UploadLocation>(sql).ToDictionary(k => k.Reference, v => v);

                if(!locationDictionary.HasContent())
                    notificationService.Log("DocumentManagerService: FetchUploadLocationDictionary: Could not load location information dictionary.");

                return locationDictionary;
            }
        }

        public UploadLocation FetchUploadLocation(UploadReference _reference)
        {
            var locations = FetchUploadLocationDictionary();

            if (!locations.HasContent())
                return null;

            UploadLocation uploadLocation;

            if (locations.TryGetValue(_reference, out uploadLocation))
                return uploadLocation;

            notificationService.AddError(string.Format("Could not load location information for {0}", _reference));
            notificationService.Log(string.Format("DocumentManagerService: FetchUploadLocation: Could not load location information for {0}", _reference));
            return null;
        }

        public UploadedDocument FetchLastUploaded(UploadReference _reference)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                const string sql = "SELECT `id`, `Name`, `Location`, `extension`, `modifiedName`, `DateUploaded`, `UploadedByUsername` FROM `fileUploads` WHERE `Reference`=@Reference ORDER BY `DateUploaded` DESC LIMIT 1;;";

                return connection.Query<UploadedDocument>(sql, new { reference = _reference }).SingleOrDefault();
            }
        }
    }
}
