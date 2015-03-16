using System;
using eMotive.Models.Objects.Users;

namespace eMotive.Services.Objects
{
    public class UploadedDocument
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public string Extension { get; set; }
        public string ModifiedName { get; set; }
        public DateTime DateUploaded { get; set; }
        public string UploadedByUsername { get; set; }
        public User UploadedBy { get; set; }
        public UploadReference Reference { get; set; }
    }
}
