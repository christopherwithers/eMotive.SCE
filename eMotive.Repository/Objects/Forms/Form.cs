using System.Collections.Generic;

namespace eMotive.Repository.Objects.Forms
{
    public class Form
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public IEnumerable<FormItem> Fields { get; set; }
    }
}
