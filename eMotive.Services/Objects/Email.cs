using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Mail;

namespace eMotive.Services.Objects
{
    public class Email
    {
        public Email()
        {
            To = new Collection<string>();
            CC = new Collection<string>();
            BCC = new Collection<string>();
            Attachments = new Collection<Attachment>();
            Priority = MailPriority.Normal;
            IsBodyHtml = true;
        }

        public ICollection<string> To { get; set; }
        public string From { get; set; }
        public ICollection<string> CC { get; set; }
        public ICollection<string> BCC { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public bool IsBodyHtml { get; set; }
        public MailPriority Priority { get; set; }

        public ICollection<Attachment> Attachments { get; set; } 

    }
}
