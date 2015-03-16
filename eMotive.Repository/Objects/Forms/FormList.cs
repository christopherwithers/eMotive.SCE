using System.Collections.Generic;

namespace eMotive.Repository.Objects.Forms
{
    public class FormList
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<FormListItem> Collection { get; set; } 
    }
}
