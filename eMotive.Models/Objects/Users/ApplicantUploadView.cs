using System.Collections.Generic;
using eMotive.Models.Objects.Signups;
using eMotive.Models.Objects.Uploads;

namespace eMotive.Models.Objects.Users
{
    public class ApplicantUploadView
    {
        public UploadedDocument LastUploadedDocument { get; set; }
        public IEnumerable<Group> Groups { get; set; }
    }
}
