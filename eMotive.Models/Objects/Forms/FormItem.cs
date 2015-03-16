namespace eMotive.Models.Objects.Forms
{
    public class FormItem
    {
        public int ID { get; set; }
        public string Field { get; set; }
        public FieldType Type { get; set; }
        public int FormId { get; set; }
        public int ListID { get; set; }
        public int Order { get; set; }
    }
}
