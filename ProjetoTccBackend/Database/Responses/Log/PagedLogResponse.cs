using System.Collections.Generic;
using ProjetoTccBackend.Database.Responses.Log;

namespace ProjetoTccBackend.Database.Responses.Log
{
    public class PagedLogResponse
    {
        public IEnumerable<LogResponse> Items { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
