using Grinding.Shared.Models;
using DataHub.Core.Models.DataGrid;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataHub.Core.Models.UI;

namespace Grinding.Services
{
    public interface IAlarmSpecificService
    {
        Task<AlarmsPagedResult?> GetAlarmsForApiPagedAsync(AlarmFilterModel filter);
        Task<Stream> ExportAlarmsToExcelAsync(DataRequestBase request, string? filterName = null, string? filterString = null, List<FilterCriterion>? filters = null, List<SortCriterion>? sorts = null);
    }
}
