namespace eMotive.Services.Objects
{
    public class EditableEmail
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public bool Custom { get; set; }
    }
}
