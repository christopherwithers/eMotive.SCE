using System.Collections.Generic;
using eMotive.Models.Objects.Uploads;
using eMotive.Services.Objects;

namespace eMotive.Services.Interfaces
{
    public interface IDocumentManagerService
    {
        MimeType FetchMimeTypeForExtension(string _extension);
        IDictionary<string, MimeType> FetchMimeTypeDictionary();

        MimeType FetchDefaultMimeType();

        bool SaveDocumentInformation(UploadedDocument _document);

        IDictionary<UploadReference, UploadLocation> FetchUploadLocationDictionary();
        UploadLocation FetchUploadLocation(UploadReference _reference);

        UploadedDocument FetchLastUploaded(UploadReference _reference);
    }
}
