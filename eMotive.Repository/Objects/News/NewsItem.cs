using System;

namespace eMotive.Repository.Objects.News
{
    public class NewsItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public int AuthorID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Image { get; set; }
        public bool Enabled { get; set; }
        public bool Archived { get; set; }
    }
}
