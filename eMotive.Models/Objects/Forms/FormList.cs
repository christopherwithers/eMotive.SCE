using System.Collections.Generic;

namespace eMotive.Models.Objects.Forms
{
    public class FormList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<FormListItem> Collection { get; set; } 
    }
}
