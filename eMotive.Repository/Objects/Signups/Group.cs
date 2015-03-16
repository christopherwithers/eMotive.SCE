namespace eMotive.Repository.Objects.Signups
{
    public class Group
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool EnableEmails { get; set; }
        public bool DisabilitySignups { get; set; }
        public bool AllowMultipleSignups { get; set; }
        public string Description { get; set; }

        public bool AllowSelfSignup { get; set; }
        public string SelfSignupDeniedMessage { get; set; }
    }
}
