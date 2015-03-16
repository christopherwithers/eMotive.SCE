namespace eMotive.Models.Objects.Uploads
{
    public enum UploadReference { BOXI, A100Applicants, A101Applicants }
    public class UploadLocation
    {
        public int ID { get; set; }
        public UploadReference Reference { get; set; }
        public string Directory { get; set; }
    }
}
