using System;

namespace eMotive.Models.Objects.Users
{
    public class ApplicantData
    {
        public int ID { get; set; }
        public string TermCode { get; set; }
        public DateTime ApplicationDate { get; set; }
        public string ApplicantID { get; set; }
        public string PersonalID { get; set; }
        public string ApplicantPrefix { get; set; }
        public string Surname { get; set; }
        public string Firstname { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string DisabilityCode { get; set; }
        public string DisabilityDesc { get; set; }
        public string ResidenceCode { get; set; }
        public string NationalityDesc { get; set; }
        public string CorrespondenceAddr1 { get; set; }
        public string CorrespondenceAddr2 { get; set; }
        public string CorrespondenceAddr3 { get; set; }
        public string CorrespondenceCity { get; set; }
        public string CorrespondenceNationDesc { get; set; }
        public string CorrespondencePostcode { get; set; }
        public string EmailAddress { get; set; }
        public string PreviousSchoolDesc { get; set; }
        public string SchoolAddressCity { get; set; }
        public string SchoolLEADescription { get; set; }

    }
}
