using System.Collections.Generic;

namespace eMotive.Models.Objects.Forms
{
    public class Form
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<FormItem> Fields { get; set; }
    }
}
