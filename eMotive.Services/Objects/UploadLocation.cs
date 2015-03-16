namespace eMotive.Services.Objects
{
    public enum UploadReference { BOXI, Applicants }
    public class UploadLocation
    {
        public int ID { get; set; }
        public UploadReference Reference { get; set; }
        public string Directory { get; set; }
    }
}
