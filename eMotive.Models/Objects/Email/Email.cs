using eMotive.Models.Validation.Email;
using ServiceStack.FluentValidation.Attributes;

namespace eMotive.Models.Objects.Email
{
    [Validator(typeof (EmailValidator))]
    public class Email
    {
        public int ID { get; set; }
        public string Key { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public bool Custom { get; set; }
    }
}
