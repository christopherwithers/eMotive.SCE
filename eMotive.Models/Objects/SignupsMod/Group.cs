namespace eMotive.Models.Objects.SignupsMod
{
    public class Group
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool DisabilitySignups { get; set; }
        public bool AllowMultipleSignups { get; set; }
        public string Description { get; set; }
    }
}
