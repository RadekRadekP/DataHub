using System.Collections.Generic;
using System.Linq;

namespace DataHub.Core.Models.DataGrid
{
    public class ServerDataResult<T>
    {
        public IQueryable<T> Data { get; set; } = Enumerable.Empty<T>().AsQueryable();
        public int TotalCount { get; set; }
        public int TotalUnfilteredCount { get; set; }
    }
}