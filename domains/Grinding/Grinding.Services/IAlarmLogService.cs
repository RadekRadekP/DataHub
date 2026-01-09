using Grinding.Shared.Models; // Assuming AlarmLogEntry is in this namespace
using DataHub.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Grinding.Services
{
    public interface IAlarmLogService
    {
        Task<List<AlarmLogEntry>> GetAlarmsAsync();
    }
}