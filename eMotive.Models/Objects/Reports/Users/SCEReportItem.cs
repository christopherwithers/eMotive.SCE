using System;

namespace eMotive.Models.Objects.Reports.Users
{
    public class SCEReportItem
    {
        public int UserID { get; set; }
        public string Username { get; set; }
        public string Title { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string GMCNumber { get; set; }
        public int MainSpecialty { get; set; }
        public string Email { get; set; }
        public string SecretaryEmail { get; set; }
        public string OtherEmail { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Postcode { get; set; }
        public string PhoneWork { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneOther { get; set; }
        public string Trust { get; set; }
        public string Grade { get; set; }
        public bool Trained { get; set; }
        public bool Enabled { get; set; }
        public string Notes { get; set; }
        public DateTime DateTrained { get; set; }
    }
}
