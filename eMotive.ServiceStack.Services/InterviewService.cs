using System.Collections.Generic;
using ServiceStack;

namespace eMotive.ServiceStack.Services
{
    [Route("/Services/Interview")]
    public class Interview
    {
        public IEnumerable<int> GroupIds { get; set; } 
    }

    public class InterviewService : Service
    {
        public object Any(Interview request)
        {
            return new InterviewResponse { Success = true, Result = null };
        }
    }

    public class InterviewResponse
    {
        public bool Success;
        public IEnumerable<int> Result { get; set; }
        public ResponseStatus ResponseStatus { get; set; } //Where Exceptions get auto-serialized
    }
}
