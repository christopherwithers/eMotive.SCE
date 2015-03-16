using System;
using ServiceStack.FluentValidation.Attributes;
using eMotive.Models.Validation.News;

namespace eMotive.Models.Objects.News
{
    [Validator(typeof(NewsValidator))]
    public class NewsItem
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public Users.User Author { get; set; }
        public int AuthorID { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public string Image { get; set; }
        public bool Enabled { get; set; }
        public bool Archived { get; set; }
    }
}
