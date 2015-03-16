using System.Collections.Generic;

namespace eMotive.Api
{
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
        public T Result { get; set; }
    }
}
