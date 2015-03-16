using System.Collections.Generic;

namespace eMotive.Models.Objects.Menu
{
    public class Menu
    {
        public int ID { get; set; }
        public string Title { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; }
    }

    public class MenuItem
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string URL { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }

        public ICollection<MenuItem> MenuItems { get; set; }
    }
}
