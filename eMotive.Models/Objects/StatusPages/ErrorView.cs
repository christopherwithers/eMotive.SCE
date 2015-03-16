using System.Collections.Generic;

namespace eMotive.Models.Objects.StatusPages
{
    public class ErrorView
    {
        private string referrer;
        
        public IEnumerable<string> Errors { get; set; }
        public string ControllerName { get; set; }
        public string Referrer {
            set { referrer = value; }
        }

        public string GetReferrer()
        {
            return !string.IsNullOrEmpty(referrer) ? referrer : "/Home/Index";
        }
    }
}
