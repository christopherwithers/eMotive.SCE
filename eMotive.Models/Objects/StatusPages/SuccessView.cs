using System.Collections.Generic;

namespace eMotive.Models.Objects.StatusPages
{
    public class SuccessView
    {
        public IEnumerable<Link> Links { get; set; }
        public string Message { get; set; }

            public class Link
            {
                public string Text { get; set; }
                public string URL { get; set; }
            }
    }


}
