namespace eMotive.Repository.Objects.Menu
{
    /// <summary>
    /// If is a parent menu item, have ID = ParentID? Else parent ID will be that of the above menu link i.e. Section
    /// </summary>
    public class MenuItem
    {
        public int ID { get; set; }
        public int Name { get; set; }
        public int ParentID { get; set; }
    }
}
