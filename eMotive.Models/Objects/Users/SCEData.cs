using System;
using System.Collections.Generic;
using eMotive.Models.Objects.Signups;
using eMotive.Models.Validation.User;
using ServiceStack.FluentValidation.Attributes;

namespace eMotive.Models.Objects.Users
{
    [Validator(typeof(SCEDataValidator))]
    public class SCEData
    {
        public int ID { get; set; }
        public int IdUser { get; set; }
        public string Username { get; set; }
        public int ExaminationNumber { get; set; }
        public string GMCNumber { get; set; }
        public string Title { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string SecretaryEmail { get; set; }
        public string EmailOther { get; set; }
        public int MainSpecialty { get; set; }
        public string Trust { get; set; }
        public string Grade { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Postcode { get; set; }
        public string PhoneWork { get; set; }
        public string PhoneMobile { get; set; }
        public string PhoneOther { get; set; }
        public string Notes { get; set; }
        public bool Trained { get; set; }
        public bool Enabled { get; set; }
        public DateTime DateTrained { get; set; }
        public string[] BelongsToGroups { get; set; }

        public IEnumerable<Group> AllGroups { get; set; }
    }
}
