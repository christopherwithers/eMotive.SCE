using System;
using ServiceStack.FluentValidation.Attributes;
using eMotive.Models.Validation.Page;

namespace eMotive.Models.Objects.Pages
{
    [Validator(typeof(PageValidator))]
    public class Page
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }
        public bool Enabled { get; set; }
        public bool Archived { get; set; }
    }
}
