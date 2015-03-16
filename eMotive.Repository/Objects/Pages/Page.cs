using System;

namespace eMotive.Repository.Objects.Pages
{
    //todo: have an updated by link?
    //todo:http://www.asp.net/mvc/tutorials/getting-started-with-ef-using-mvc/sorting-filtering-and-paging-with-the-entity-framework-in-an-asp-net-mvc-application
    public class Page
    {//https://github.com/TroyGoode/PagedList
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Enabled { get; set; }
        public bool Archived { get; set; }
    }
}
